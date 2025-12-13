using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Promptino.Core.Mappings;
using Promptino.Core.Options;
using Promptino.Core.ServiceContracts;
using Promptino.Core.ServiceContracts.CategoryServiceContracts;
using Promptino.Core.ServiceContracts.ImageServiceContracts;
using Promptino.Core.Services;
using Promptino.Core.Services.CategoryServices;
using Promptino.Core.Services.ImageServices;
using Promptino.Core.Services.PromptServices;
using Promptino.Core.Validators.PromptValidators;

namespace Promptino.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(typeof(PromptProfile).Assembly);

        services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
        services.AddValidatorsFromAssemblyContaining<PrormptAddRerquestValidator>();
        services.AddScoped<ITokenService, TokenService>();

        services.AddScoped<IImageGetterrService, ImageGetterService>();
        services.AddScoped<IImageAdderService, ImageAdderService>();
        services.AddScoped<IImageUpdaterService, ImageUpdaterService>();
        services.AddScoped<IImageDeleterService, ImageDeleterService>();

        services.AddScoped<IPromptGetterService, PromptGetterService>();
        services.AddScoped<IPromptAdderService, PromptAdderService>();
        services.AddScoped<IPromptUpdaterService, PromptUpdaterService>();
        services.AddScoped<IPromptDeleterService, PromptDeleterService>();

        services.AddScoped<ICategoryGetterService, CategoryGetterService>();
        services.AddScoped<ICategoryAdderService, CategoryAdderService>();
        services.AddScoped<ICategoryUpdaterService, CategoryUpdaterService>();
        services.AddScoped<ICategoryDeleterService, CategoryDeleterService>();



        services.Configure<JwtOptions>(configuration.GetSection(nameof(JwtOptions)));


        return services;
    }
}
