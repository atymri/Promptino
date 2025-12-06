using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Promptino.Core.Exceptions;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Promptino.API.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        int statusCode = StatusCodes.Status500InternalServerError;
        string title = "خطایی رخ داده است.";
        string detail = exception.Message;

        switch (exception)
        {
            case ImageNotFoundException:
            case PromptNotFoundExceptions:
                statusCode = StatusCodes.Status404NotFound;
                title = "مورد پیدا نشد";
                break;

            case ImageExistsException:
            case PromptExistsException:
                statusCode = StatusCodes.Status409Conflict;
                title = "مورد از قبل وجود دارد";
                break;

            case NullImageForPromptException:
            case ImageLimitException:
            case InvalidPromptIdException:
                statusCode = StatusCodes.Status400BadRequest;
                title = "درخواست نامعتبر";
                break;

            default:
                #if DEBUG
                detail = exception.ToString(); // stacktrace در توسعه
                #endif
                break;
        }

        context.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Type = $"https://httpstatuses.com/{statusCode}"
        };

        var json = JsonSerializer.Serialize(problemDetails);

        return context.Response.WriteAsync(json);
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
