using IdentityPrvd.Common.Api;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Helpers;
using IdentityPrvd.Services.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IdentityPrvd.Infrastructure.Middleware;

public class GlobalExceptionHandlerMiddleware(
    ILogger<GlobalExceptionHandlerMiddleware> logger,
    ILocalizationService localizationService,
    IIdentityContext identityContext) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        try
        {
            await next(context);
        }
        catch (ValidationFailedException ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            await context.Response.WriteAsJsonAsync(ApiResponse.ValidationFail(ex.Errors), Settings.Json);
        }
        catch (HttpResponseException ex)
        {
            logger.LogWarning(ex, ex.Message);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex.StatusCode;
            
            string message = ex.Message;
            if (!string.IsNullOrEmpty(ex.LocalizationKey))
            {
                var language = identityContext.CurrentLanguage ?? "en";
                message = ex.LocalizationArgs != null && ex.LocalizationArgs.Length > 0
                    ? localizationService.GetString(ex.LocalizationKey, language, ex.LocalizationArgs)
                    : localizationService.GetString(ex.LocalizationKey, language);
            }
            
            await context.Response.WriteAsJsonAsync(ApiResponse.Fail(message), Settings.Json);
        }
        catch (BadHttpRequestException ex)
        {
            logger.LogWarning(ex, ex.Message);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            var language = identityContext.CurrentLanguage ?? "en";
            var message = localizationService.GetString("errors.bad_request", language);
            await context.Response.WriteAsJsonAsync(ApiResponse.Fail(message), Settings.Json);
        }
        catch (NotImplementedException nie)
        {
            logger.LogError(nie, nie.Message);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            var language = identityContext.CurrentLanguage ?? "en";
            var message = localizationService.GetString("errors.not_implemented", language);
            await context.Response.WriteAsJsonAsync(ApiResponse.Fail(message), Settings.Json);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            var language = identityContext.CurrentLanguage ?? "en";
            var message = localizationService.GetString("errors.server.error", language);
            await context.Response.WriteAsJsonAsync(ApiResponse.Fail(ex.StackTrace), Settings.Json);
        }
    }
}
