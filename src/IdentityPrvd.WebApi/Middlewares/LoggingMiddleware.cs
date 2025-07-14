using IdentityPrvd.WebApi.Logging;
using Microsoft.AspNetCore.Identity.Data;
using System.Diagnostics;

namespace IdentityPrvd.WebApi.Middlewares;

public class LoggingMiddleware(ILogger<LoggingMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        LogRequest(context.Request);
        var stopwatch = Stopwatch.StartNew();
        await next(context);
        LogResponse(context.Request, context.Response, stopwatch.ElapsedMilliseconds);
    }

    private void LogRequest(HttpRequest request)
    {
        var logRequest = RequestInfo.Create(request);
        logger.LogInformation("RequestInfo: {logRequest}", logRequest);
    }

    private void LogResponse(HttpRequest request, HttpResponse response, long elapsedMilliseconds)
    {
        var logResponse = ResponseInfo.Create(request, response, elapsedMilliseconds);
        logger.LogInformation("ResponseInfo: {logResponse}", logResponse);
    }
}
