using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Core.Domain.RerpositoryContracts;
using Promptino.Core.Services.PromptImageServices;
using Promptino.Infrastructure.DatabaseContext;
using Promptino.Infrastructure.Repositories;
using Promptino.Infrastructure.Services;

namespace Promptino.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, 
        IConfiguration configuration)
    {
        var connectionStrring = configuration.GetConnectionString("Default")
            ?? throw new KeyNotFoundException("connection string is null!");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionStrring);
        });

        services.AddHostedService<RoleInitializationService>();

        services.AddScoped<IPromptRepository, PromptRepository>();
        services.AddScoped<IImageRepository, ImageRepositorry>();
        services.AddScoped<IPromptImageRepository, PromptImageRepository>();

        return services;
    }
}
