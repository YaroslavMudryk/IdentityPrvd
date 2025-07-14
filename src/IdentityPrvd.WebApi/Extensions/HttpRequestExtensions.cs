namespace IdentityPrvd.WebApi.Extensions;

public static class HttpRequestExtensions
{
    public static string GetValueOrNull(this IHeaderDictionary headers, string key)
    {
        ArgumentNullException.ThrowIfNull(headers);

        return headers[key].FirstOrDefault()!;
    }
}
