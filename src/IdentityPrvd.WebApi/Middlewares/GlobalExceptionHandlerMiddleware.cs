using FluentValidation;
using IdentityPrvd.WebApi.Api;
using IdentityPrvd.WebApi.Exceptions;
using IdentityPrvd.WebApi.Helpers;

namespace IdentityPrvd.WebApi.Middlewares;

public class GlobalExceptionHandlerMiddleware(ILogger<GlobalExceptionHandlerMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            var validationErrors = ex.Errors
                    .ToLookup(x => x.PropertyName, x => x.ErrorMessage)
                    .ToDictionary(x => x.Key, x => x.ToArray());

            await context.Response.WriteAsJsonAsync(ApiResponse.ValidationFail(validationErrors), Settings.Json);
        }
        catch (HttpResponseException ex)
        {
            logger.LogWarning(ex, ex.Message);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex.StatusCode;
            await context.Response.WriteAsJsonAsync(ApiResponse.Fail(ex.Message), Settings.Json);
        }
        catch (BadHttpRequestException ex)
        {
            logger.LogWarning(ex, ex.Message);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(ApiResponse.Fail("Bad request"), Settings.Json);
        }
        catch (NotImplementedException nie)
        {
            logger.LogError(nie, nie.Message);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(ApiResponse.Fail("This functionality is planned but not yet implemented, it will be available in the near future"), Settings.Json);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(ApiResponse.Fail("Server error"), Settings.Json);
        }
    }
}
