namespace IdentityPrvd.Options;

public class ExternalProviderOptions
{
    public bool IsAvailable { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string Icon { get; set; }
    public string AuthenticationScheme { get; set; }
    public string CallbackPath { get; set; }
    public List<string> Scopes { get; set; } = [];
    public Dictionary<string, string> AdditionalOptions { get; set; } = [];
}
