using System.Text.Json.Serialization;

namespace IdentityPrvd.Features.Authentication.ExternalSignin.Dtos;

public class FacebookProfileInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("email")]
    public string Email { get; set; }
    [JsonPropertyName("picture")]
    public Picture Picture { get; set; }
    [JsonPropertyName("first_name")]
    public string FirstName { get; set; }
    [JsonPropertyName("last_name")]
    public string LastName { get; set; }
    [JsonPropertyName("languages")]
    public List<Lang> Languages { get; set; }
}

public class Picture
{
    [JsonPropertyName("data")]
    public Data Data { get; set; }
}

public class Data
{
    [JsonPropertyName("height")]
    public int Height { get; set; }
    [JsonPropertyName("is_silhouette")]
    public bool IsSilhouette { get; set; }
    [JsonPropertyName("url")]
    public string Url { get; set; }
    [JsonPropertyName("width")]
    public int Width { get; set; }
}

public class Lang
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
}
