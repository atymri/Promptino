using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Promptino.Core.Mappings;
using Promptino.Core.ServiceContracts.ImageServiceContracts;
using Promptino.Core.Services.ImageServices;
using Promptino.Core.Services.PromptServices;
using Promptino.Core.Validators.PromptValidators;

namespace Promptino.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(PromptProfile).Assembly);

        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<PrormptAddRerquestValidator>();

        services.AddScoped<IImageGetterrService, ImageGetterService>();
        services.AddScoped<IImageAdderService, ImageAdderService>();
        services.AddScoped<IImageUpdaterService, ImageUpdaterService>();
        services.AddScoped<IImageDeleterService, ImageDeleterService>();

        services.AddScoped<IPromptGetterService, PromptGetterService>();
        services.AddScoped<IPromptAdderService, PromptAdderService>();
        services.AddScoped<IPromptUpdaterService, PromptUpdaterService>();
        services.AddScoped<IPromptDeleterService, PromptDeleterService>();


        return services;
    }
}
