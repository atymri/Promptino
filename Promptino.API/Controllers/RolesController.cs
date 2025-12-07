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

    // GET: roles
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
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

    // POST: roles/create
    [HttpPost("create")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateRole(CreateRoleDto dto)
    {
        if (!ModelState.IsValid)
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

    // POST: roles/add-user-to-role
    [HttpPost("add-user-to-role")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

    // POST: roles/remove-user-from-role
    [HttpPost("remove-user-from-role")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveUserFromRole(RemoveUserFromRoleDto dto)
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

        var isInRole = await _userManager.IsInRoleAsync(user, dto.RoleName);
        if (!isInRole)
            return Problem(
                detail: $"کاربر '{user.UserName}' در نقش '{dto.RoleName}' نیست.",
                statusCode: 400,
                title: "خطای اعتبارسنجی"
            );

        var result = await _userManager.RemoveFromRoleAsync(user, dto.RoleName);

        if(result.Succeeded)
            return Ok(new { message = $"کاربر '{user.UserName}' با موفقیت از نقش '{dto.RoleName}' حذف شد." });

        return Problem(
            detail: string.Join(", ", result.Errors.Select(e => e.Description)),
            statusCode: 500,
            title: "خطای سرور"
        );
    }

    // DELETE: roles/delete
    [HttpDelete("delete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteRole(DeleteRoleDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var role = await _roleManager.FindByNameAsync(dto.RoleName);
        if (role == null)
            return Problem(
                detail: $"نقش '{dto.RoleName}' پیدا نشد.",
                statusCode: 404,
                title: "خطای یافت نشد"
            );

        var result = await _roleManager.DeleteAsync(role);

        if (result.Succeeded)
            return Ok(new { message = $"نقش '{dto.RoleName}' با موفقیت حذف شد." });

        return Problem(
            detail: string.Join(", ", result.Errors.Select(e => e.Description)),
            statusCode: 500,
            title: "خطای سرور"
        );
    }
}
