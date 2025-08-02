namespace IdentityPrvd.Options;

public class IdentityPrvdOptions
{
    public bool TrackSessionActivity { get; set; } = true;
    public IdentityConnectionOptions Connections { get; set; } = new IdentityConnectionOptions();
    public AppOptions App { get; set; } = new AppOptions();
    public ProtectionOptions Protection { get; set; } = new ProtectionOptions();
    public TokenOptions Token { get; set; } = new TokenOptions();
    public Dictionary<string, ExternalProviderOptions> ExternalProviders { get; set; } = [];
    public UserOptions User { get; set; } = new UserOptions();
    public PasswordOptions Password { get; set; } = new PasswordOptions();
    public LanguageOptions Language { get; set; } = new LanguageOptions();
}
