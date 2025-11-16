using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Contexts;
using Microsoft.AspNetCore.Http;

namespace IdentityPrvd.Infrastructure.Middleware;

public class CorrelationContextMiddleware(IIdentityContext identityContext) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        ArgumentNullException.ThrowIfNull(identityContext);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var correlationId = context.GetOrAssignCorrelationId();

        ((IdentityContext)identityContext).IpAddress = context.Connection.LocalIpAddress != null ?
            context.Connection.LocalIpAddress.ToString() : context.Connection.RemoteIpAddress != null ? context.Connection.RemoteIpAddress.ToString() : "localhost";
        ((IdentityContext)identityContext).CorrelationId = correlationId;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers.Append(HttpConstants.CorrelationId, correlationId);
            return Task.CompletedTask;
        });

        await next(context);
    }
}
