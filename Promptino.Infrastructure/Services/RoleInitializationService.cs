using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Promptino.Core.Domain.Entities;
using Promptino.Core.ServiceContracts;

namespace Promptino.Infrastructure.Services;

public class RoleInitializationService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RoleInitializationService> _logger;
    
    public RoleInitializationService(
        IServiceProvider serviceProvider,
        ILogger<RoleInitializationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        await InitializeRolesAsync(roleManager);
        await CreateAdminUserAsync(userManager, roleManager, tokenService);
    }

    private async Task InitializeRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        try
        {
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                var adminRole = new ApplicationRole
                {
                    Name = "Admin",
                    Details = "مدیر سیستم - دسترسی کامل به تمامی بخش‌های سیستم"
                };

                await roleManager.CreateAsync(adminRole);
                _logger.LogInformation("Added ADMIN Role");
            }
            else
            {
                _logger.LogInformation("ADMIN Role Already Exists");
            }

            if (!await roleManager.RoleExistsAsync("User"))
            {
                var userRole = new ApplicationRole
                {
                    Name = "User",
                    Details = "کاربر عادی - دسترسی به امکانات پایه سیستم"
                };

                await roleManager.CreateAsync(userRole);
                _logger.LogInformation("Added USER Role");
            }
            else
            {
                _logger.LogInformation("USER Role Already Exists");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ERROR in adding primary roles");
            throw;
        }
    }

    private async Task CreateAdminUserAsync(UserManager<ApplicationUser> userManager, 
        RoleManager<ApplicationRole> roleManager, ITokenService tokenService)
    {
        try
        {
            var adminEmail = "promptinoadmin@gmail.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    FirstName = "مدیر",
                    LastName = "سیستم",
                    RefreshToken = string.Empty,
                    RefreshTokenExpiration = DateTime.MinValue
                };

                var password = "4sB4bId4RcH4M4N@123";

                var result = await userManager.CreateAsync(admin, password);

                if (result.Succeeded)
                {
                    var token = tokenService.CreateToken(admin);

                    admin.RefreshToken = token.RefreshToken;
                    admin.RefreshTokenExpiration = token.RefreshTokenExpiry;

                    await userManager.UpdateAsync(admin);

                    if (!await roleManager.RoleExistsAsync("Admin"))
                    {
                        await InitializeRolesAsync(roleManager);
                    }

                    await userManager.AddToRoleAsync(admin, "Admin");

                    _logger.LogInformation("Admin user created successfully with username: {Username}", admin.UserName);
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to create admin user. Errors: {Errors}", errors);
                }
            }
            else
            {
                _logger.LogInformation("Admin user already exists");

                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    _logger.LogInformation("Added Admin role to existing admin user");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ERROR in creating admin user");
            throw;
        }
    }


    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}