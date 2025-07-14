using IdentityPrvd.WebApi.Constants;
using IdentityPrvd.WebApi.CurrentContext;
using IdentityPrvd.WebApi.Extensions;

namespace IdentityPrvd.WebApi.Middlewares;

public class CorrelationContextMiddleware(ICurrentContext currentContext) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        ArgumentNullException.ThrowIfNull(currentContext);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var correlationId = context.GetOrAssignCorrelationId();

        ((CurrentContext.CurrentContext)currentContext).IpAddress = context.Connection.LocalIpAddress != null ?
            context.Connection.LocalIpAddress.ToString() : context.Connection.RemoteIpAddress != null ? context.Connection.RemoteIpAddress.ToString() : "localhost";
        ((CurrentContext.CurrentContext)currentContext).CorrelationId = correlationId;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers.Append(HttpConstants.CorrelationId, correlationId);
            return Task.CompletedTask;
        });

        await next(context);
    }
}
