namespace IdentityPrvd.Options;

public class ProtectionOptions
{
    public string Key { get; set; }    
    public RateLimitOptions RateLimit { get; set; } = new();
}
