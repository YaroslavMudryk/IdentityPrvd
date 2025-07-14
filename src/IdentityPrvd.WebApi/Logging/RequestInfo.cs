using IdentityPrvd.WebApi.Extensions;

namespace IdentityPrvd.WebApi.Logging;

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
