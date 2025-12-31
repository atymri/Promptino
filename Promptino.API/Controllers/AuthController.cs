using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Promptino.API.Tools;
using Promptino.Core.Domain.Entities;
using Promptino.Core.DTOs;
using Promptino.Core.ServiceContracts;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;

namespace Promptino.API.Controllers;

[AllowAnonymous]
public class AuthController : BaseController
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly HtmlToString _htmlToString;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public AuthController(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signinManager,
        HtmlToString htmlToString,
        IMapper mapper,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signinManager;
        _mapper = mapper;
        _htmlToString = htmlToString;
        _tokenService = tokenService;
    }

    // POST: auth/register
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegisterResponse>> Register(RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (await _userManager.FindByEmailAsync(request.Email) is not null)
        {
            ModelState.AddModelError("Email", "حساب کاربری وجود دارد.");
            return ValidationProblem(ModelState);
        }

        var user = _mapper.Map<ApplicationUser>(request);
        user.UserName = request.Email;

        var res = await _userManager.CreateAsync(user, request.Password);

        if (!res.Succeeded)
        {
            bool hasPasswordError = false;

            foreach (var error in res.Errors)
            {
                if (error.Code.Contains("Password"))
                {
                    ModelState.AddModelError("Password", error.Description);
                    hasPasswordError = true;
                }
            }

            if (hasPasswordError)
                return ValidationProblem(ModelState);

            return Problem(string.Join(", ", res.Errors.Select(e => e.Description)),
                           statusCode: StatusCodes.Status400BadRequest,
                           title: "خطای سرور");
            }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        string? callbackUrl = Url.ActionLink(nameof(ConfirmEmail), "Auth", 
            new { userId = user.Id, token = token }, Request.Scheme);

        var body = await _htmlToString.LoadAsync(
                "AccountVerification.html",
                new Dictionary<string, string>
                {
                    ["FirstName"] = user.FirstName,
                    ["LastName"] = user.LastName,
                    ["VerificationLink"] = callbackUrl,
                    ["Year"] = DateTime.UtcNow.Year.ToString()
                }
            );

        try
        {
            await EmailSender.SendAsync(new Models.EmailModel(user.Email, "تایید حساب کاربری", body));
        }
        catch
        {
            return new RegisterResponse(user.Email!, 
                false, "خطا در ارسال ایمیل, لطفا بعدا امتحان کنید");
        }
        return new RegisterResponse(user.Email!, 
            true, "ایمیل تایید ارسال شد, لطفا ایمیل های خود را برسی کنید");
    }

    [HttpGet]
    public async Task<ActionResult<AuthResponse>> ConfirmEmail(string userId, string token)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            return Problem(
                title: "درخواست نامعتبر",
                detail: "شناسه کاربر یا توکن ارسال نشده است.",
                statusCode: StatusCodes.Status400BadRequest
            );

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return Problem(
                title: "کاربر یافت نشد",
                detail: "کاربری با این شناسه وجود ندارد.",
                statusCode: StatusCodes.Status404NotFound
            );

        var decodedToken = Encoding.UTF8.GetString(
            WebEncoders.Base64UrlDecode(token)
        );

        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

        if (!result.Succeeded)
            return Problem(
                title: "تأیید ایمیل ناموفق",
                detail: "توکن نامعتبر، منقضی یا قبلاً استفاده شده است.",
                statusCode: StatusCodes.Status409Conflict
            );


        await _signInManager.SignInAsync(user, isPersistent: false);
        var signInResponse= _tokenService.CreateToken(user);

        return signInResponse;
    }

    [HttpPost("forget-password")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgetPassword([FromBody] ForgotPasswordRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return BadRequest("درخواست نامعتبر");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        var param= new Dictionary<string, string?>()
        {
            {"token", token },
            {"email", request.Email}
        };

        var callback = QueryHelpers.AddQueryString(request.ClientUri, param);
        var body = await _htmlToString.LoadAsync(
                "ResetPassword.html",
                new Dictionary<string, string>
                {
                    ["FirstName"] = user.FirstName,
                    ["LastName"] = user.LastName,
                    ["ResetLink"] = callback,
                    ["Year"] = DateTime.UtcNow.Year.ToString()
                }
            );

        try
        {
            await EmailSender.SendAsync(new Models.EmailModel(request.Email, "بازیابی رمز عبور", body));
        }
        catch
        {
            return Problem("خطا در ارسال ایمیل", "مشکلی در ارسال ایمیل بازیابی رمز عبور پیش آمده است",
                statusCode: StatusCodes.Status500InternalServerError);
        }
        return Ok();
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return BadRequest("درخواست نامعتبر");

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
        var result = await _userManager.ResetPasswordAsync(user, decodedToken, request.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return Problem("خطا در بازیابی رمز عبور", string.Join('|', errors),
                statusCode: StatusCodes.Status400BadRequest);
        }

        return Ok();
    }

    // POST: auth/login
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            ModelState.AddModelError("Email", "کاربری با این ایمیل یافت نشد.");
            return ValidationProblem(ModelState);
        }

        if(!user.EmailConfirmed)
        {
            ModelState.AddModelError("Email", "لطفا ایمیل حساب کاربری خود را تایید کنید");
            return ValidationProblem(ModelState);
        }

        if (user.LockoutEnabled &&
            user.LockoutEnd.HasValue &&
            user.LockoutEnd.Value > DateTimeOffset.UtcNow)
        {
            var remaining = user.LockoutEnd.Value - DateTimeOffset.UtcNow;
            ModelState.AddModelError("Email", $"حساب کاربری شما قفل شده است. لطفاً بعد از {remaining.Minutes} دقیقه دوباره تلاش کنید.");
            return ValidationProblem(ModelState);
        }

        var res = await _signInManager.PasswordSignInAsync(
            user,
            request.Password,
            isPersistent: request.RememberMe,
            lockoutOnFailure: true);

        if (!res.Succeeded)
        {
            if (res.IsLockedOut)
            {
                var lockoutMinutes = 5 * user.LockoutMultiplier;
                user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(lockoutMinutes);
                user.LockoutMultiplier *= 2;
                await _userManager.UpdateAsync(user);

                ModelState.AddModelError("Password", $"رمز عبور اشتباه است. حساب شما برای {lockoutMinutes} دقیقه قفل شد.");
            }
            else
            {
                ModelState.AddModelError("Password", "رمز عبور اشتباه است.");
            }

            return ValidationProblem(ModelState);
        }

        if (user.LockoutMultiplier > 1)
        {
            user.LockoutMultiplier = 1;
            await _userManager.UpdateAsync(user);
        }

        var response = _tokenService.CreateToken(user);

        return response;
    }

    // GET: auth/logout
    [HttpGet("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return NoContent();
    }

    // POST: auth/new-access-token
    [HttpPost("new-access-token")]
    [Authorize]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> NewAccessToken(RefreshTokenRequest request)
    {
        if (request is null)
            return Problem("درخواست نامعتبر است.",
                           statusCode: StatusCodes.Status400BadRequest,
                           title: "خطای درخواست");

        var claims = _tokenService.GetPrincipalFromToken(request.Token);
        if (claims is null)
            return Problem("توکن دسترسی نامعتبر است.",
                           statusCode: StatusCodes.Status400BadRequest,
                           title: "خطای درخواست");

        var email = claims.FindFirstValue(ClaimTypes.Name);
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiration <= DateTime.UtcNow)
            return Problem("Refresh Token نامعتبر است.",
                           statusCode: StatusCodes.Status400BadRequest,
                           title: "خطای درخواست");

        var response = _tokenService.CreateToken(user);
        user.RefreshToken = response.RefreshToken;
        user.RefreshTokenExpiration = response.RefreshTokenExpiry;

        await _userManager.UpdateAsync(user);

        return response;
    }

}
