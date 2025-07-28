using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace IdentityPrvd.Infrastructure.Middleware;

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

public record RequestInfo(
    string Method,
    string Path,
    HostString Host,
    long? ContentLength,
    string ContentType,
    string QueryString
)
{
    public static RequestInfo Create(HttpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var headers = request.Headers;
        return new RequestInfo(
            request.Method,
            request.Path,
            request.Host,
            request.ContentLength,
            request.ContentType!,
            request.QueryString.ToString()
        );
    }
}

public record ResponseInfo(
    string Method,
    string Path,
    HostString Host,
    string QueryString,
    int StatusCode,
    string ContentType,
    long ElapsedMilliseconds)
{
    public static ResponseInfo Create(HttpRequest request, HttpResponse response, long elapsedMilliseconds)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(response);

        return new ResponseInfo(
            request.Method,
            request.Path,
            request.Host,
            request.QueryString.ToString(),
            response.StatusCode,
            response.ContentType,
            elapsedMilliseconds);
    }
}
