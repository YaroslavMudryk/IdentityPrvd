using IdentityPrvd.Common.Constants;
using Microsoft.AspNetCore.Http;

namespace IdentityPrvd.Common.Extensions;

public static class HttpContextExtensions
{
    public static string GetCorreletionId(this HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        return httpContext.Request.Headers[HttpConstants.CorrelationId]!;
    }

    public static string GetOrAssignCorrelationId(this HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var correlationId = httpContext.GetCorreletionId();

        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N");
            httpContext.Request.Headers.Append(HttpConstants.CorrelationId, correlationId);
        }

        return correlationId;
    }
}
