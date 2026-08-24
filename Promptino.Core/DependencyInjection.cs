using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Promptino.Core.Mappings;
using Promptino.Core.Options;
using Promptino.Core.ServiceContracts;
using Promptino.Core.ServiceContracts.CategoryServiceContracts;
using Promptino.Core.ServiceContracts.CommentServiceContracts;
using Promptino.Core.ServiceContracts.ImageServiceContracts;
using Promptino.Core.ServiceContracts.PromptReactionServiceContracts;
using Promptino.Core.ServiceContracts.SavedPromptServiceContracts;
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
        services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(typeof(PromptProfile).Assembly);
        });

        services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
        services.AddValidatorsFromAssemblyContaining<PrormptAddRerquestValidator>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEmailService, EmailService>();

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

        services.AddScoped<ISavedPromptAdderService, Promptino.Core.Services.SavedPromptServices.SavedPromptAdderService>();
        services.AddScoped<ISavedPromptDeleterService, Promptino.Core.Services.SavedPromptServices.SavedPromptDeleterService>();
        services.AddScoped<ISavedPromptGetterService, Promptino.Core.Services.SavedPromptServices.SavedPromptGetterService>();

        services.AddScoped<IPromptReactionSetterService, Promptino.Core.Services.PromptReactionServices.PromptReactionSetterService>();
        services.AddScoped<IPromptReactionRemoverService, Promptino.Core.Services.PromptReactionServices.PromptReactionRemoverService>();
        services.AddScoped<IPromptReactionGetterService, Promptino.Core.Services.PromptReactionServices.PromptReactionGetterService>();

        services.AddScoped<ICommentAdderService, Promptino.Core.Services.CommentServices.CommentAdderService>();
        services.AddScoped<ICommentDeleterService, Promptino.Core.Services.CommentServices.CommentDeleterService>();
        services.AddScoped<ICommentGetterService, Promptino.Core.Services.CommentServices.CommentGetterService>();
        services.AddScoped<ICommentLikeSetterService, Promptino.Core.Services.CommentServices.CommentLikeSetterService>();
        services.AddScoped<ICommentLikeRemoverService, Promptino.Core.Services.CommentServices.CommentLikeRemoverService>();



        services.Configure<JwtOptions>(configuration.GetSection(nameof(JwtOptions)));
        services.Configure<EmailCredentials>(configuration.GetSection(nameof(EmailCredentials)));


        return services;
    }
}
