using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Promptino.Core.Domain.RepositoryContracts;
using Promptino.Infrastructure.DatabaseContext;
using Promptino.Infrastructure.Repositories;

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

        services.AddScoped<IPromptRepository, PromptRepository>();
        services.AddScoped<IImageRepository, ImageRepositorry>();

        return services;
    }
}
