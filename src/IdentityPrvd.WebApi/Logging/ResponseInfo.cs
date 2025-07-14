namespace IdentityPrvd.WebApi.Logging;

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
