using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Promptino.API.Controllers;
using Promptino.Core.Domain.Entities;
using Promptino.Core.DTOs;

[Authorize(Roles = "Admin")]
public class RolesController : BaseController
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<IdentityUser> _userManager;

    public RolesController(RoleManager<ApplicationRole> roleManager, UserManager<IdentityUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    // roles
    [HttpGet]
    public IActionResult GetRoles()
    {
        var roles = _roleManager.Roles.Select(r => new
        {
            r.Id,
            r.Name,
            r.Details
        });
        return Ok(roles);
    }

    // roles/create
    [HttpPost("create")]
    public async Task<IActionResult> CreateRole(CreateRoleDto dto)
    {
        if(!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (string.IsNullOrWhiteSpace(dto.RoleName))
            return Problem(
                detail: "نام نقش نمی‌تواند خالی باشد.",
                statusCode: 400,
                title: "خطای اعتبارسنجی"
            );

        var roleExist = await _roleManager.RoleExistsAsync(dto.RoleName);
        if (roleExist)
            return Problem(
                detail: $"نقش '{dto.RoleName}' از قبل وجود دارد.",
                statusCode: 400,
                title: "خطای اعتبارسنجی"
            );

        var role = new ApplicationRole
        {
            Name = dto.RoleName,
            Details = dto.Details
        };

        var result = await _roleManager.CreateAsync(role);
        if (result.Succeeded)
            return Ok(new { message = $"نقش '{dto.RoleName}' با موفقیت ایجاد شد." });

        return Problem(
            detail: string.Join(", ", result.Errors.Select(e => e.Description)),
            statusCode: 500,
            title: "خطای سرور"
        );
    }

    // roles/add-user-to-role
    [HttpPost("add-user-to-role")]
    public async Task<IActionResult> AddUserToRole(AddUserToRoleDto dto)
    {

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
        if (user == null)
            return Problem(
                detail: "کاربر مورد نظر پیدا نشد.",
                statusCode: 404,
                title: "خطای یافت نشد"
            );

        var roleExist = await _roleManager.RoleExistsAsync(dto.RoleName);
        if (!roleExist)
            return Problem(
                detail: $"نقش '{dto.RoleName}' وجود ندارد.",
                statusCode: 400,
                title: "خطای اعتبارسنجی"
            );

        var result = await _userManager.AddToRoleAsync(user, dto.RoleName);
        if (result.Succeeded)
            return Ok(new { message = $"کاربر '{user.UserName}' با موفقیت به نقش '{dto.RoleName}' اضافه شد." });

        return Problem(
            detail: string.Join(", ", result.Errors.Select(e => e.Description)),
            statusCode: 500,
            title: "خطای سرور"
        );
    }
}
