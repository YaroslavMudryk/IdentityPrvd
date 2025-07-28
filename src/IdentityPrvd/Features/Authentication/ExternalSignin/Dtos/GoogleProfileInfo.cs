using System.Text.Json.Serialization;

namespace IdentityPrvd.Features.Authentication.ExternalSignin.Dtos;

public class GoogleProfileInfo
{
    [JsonPropertyName("sub")]
    public string Id { get; set; }
    [JsonPropertyName("name")]
    public string FullName { get; set; }
    [JsonPropertyName("given_name")]
    public string FirstName { get; set; }
    [JsonPropertyName("family_name")]
    public string LastName { get; set; }
    [JsonPropertyName("picture")]
    public string Picture { get; set; }
    [JsonPropertyName("email")]
    public string Email { get; set; }
    [JsonPropertyName("email_verified")]
    public bool EmailVerified { get; set; }
}
