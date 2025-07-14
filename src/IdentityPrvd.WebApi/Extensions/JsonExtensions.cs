using IdentityPrvd.WebApi.Helpers;
using System.Text.Json;

namespace IdentityPrvd.WebApi.Extensions;

public static class JsonExtensions
{
    public static string ToJson(this object obj)
    {
        return JsonSerializer.Serialize(obj, Settings.EntityFramework);
    }

    public static T FromJson<T>(this string content)
    {
        return JsonSerializer.Deserialize<T>(content, Settings.EntityFramework)!;
    }
}
