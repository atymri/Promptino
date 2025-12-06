using EFCore.AutomaticMigrations;
using Microsoft.Extensions.DependencyInjection;
using Promptino.Infrastructure.DatabaseContext;

namespace Promptino.Infrastructure;

public static class AutoMigrationHelper
{
    public static async Task ApplyPendingMigrationsAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var options = new DbMigrationsOptions()
        {
            AutomaticMigrationsEnabled = true,
            AutomaticMigrationDataLossAllowed = true, // for delete or update queries.
            ResetDatabaseSchema = false
        };

        await context.MigrateToLatestVersionAsync(options);
    }
}