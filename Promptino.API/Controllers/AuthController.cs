using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Promptino.Core.Domain.Entities;
using Promptino.Core.DTOs;
using Promptino.Core.ServiceContracts;

namespace Promptino.API.Controllers;

public class AuthController : BaseController
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public AuthController(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signinManager,
        RoleManager<ApplicationRole> roleManager,
        IMapper mapper,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signinManager;
        _roleManager = roleManager;
        _mapper = mapper;
        _tokenService = tokenService;
    }

    // auth/register
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if(await _userManager.FindByEmailAsync(request.Email) is not null)
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

        await _signInManager.SignInAsync(user, isPersistent: false);
        var response = _tokenService.CreateToken(user);

        return response;
    }

    // auth/login
    [HttpPost("login")]
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
            isPersistent: false,
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

}
