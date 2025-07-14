namespace IdentityPrvd.WebApi.Options;

public class TokenOptions
{
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int LifeTimeInMinutes { get; set; }
    public int RefreshLifeTimeInDays { get; set; }
    public int RefreshTokenExpireWindowInMinutes { get; set; }
    public int SessionLifeTimeInDays { get; set; }
    public string SecretKey { get; set; }
}
