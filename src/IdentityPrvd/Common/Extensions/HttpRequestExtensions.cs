using Microsoft.AspNetCore.Http;

namespace IdentityPrvd.Common.Extensions;

public static class HttpRequestExtensions
{
    public static string GetValueOrNull(this IHeaderDictionary headers, string key)
    {
        ArgumentNullException.ThrowIfNull(headers);

        return headers[key].FirstOrDefault()!;
    }
}
