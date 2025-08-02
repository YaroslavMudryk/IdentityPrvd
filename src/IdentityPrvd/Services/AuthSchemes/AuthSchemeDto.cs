namespace IdentityPrvd.Services.AuthSchemes;

public class AuthSchemeDto
{
    public string Provider { get; set; }
    public bool IsAvailable { get; set; }
    public string Icon { get; set; }
    public bool IsConfigured { get; set; }
}
