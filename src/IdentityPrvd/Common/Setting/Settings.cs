using System.Text.Json;
using System.Text.Json.Serialization;

namespace IdentityPrvd.Helpers;

public static class Settings
{
    public static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
    };

    public static readonly JsonSerializerOptions EntityFramework = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
