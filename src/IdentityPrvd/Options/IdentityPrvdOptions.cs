namespace IdentityPrvd.Options;

/// <summary>
/// Configuration options for IdentityPrvd
/// </summary>
public class IdentityPrvdOptions
{
    public IdentityPrvdOptions()
    {
        Connections = new IdentityConnectionOptions();
        ExternalProviders = [];

        Token = new TokenOptions();
        User = new UserOptions();
        Language = new LanguageOptions();
        App = new AppOptions();
        Password = new PasswordOptions();
        Protection = new ProtectionOptions();
    }

    public IdentityConnectionOptions Connections { get; set; }
    public TokenOptions Token { get; set; }
    public Dictionary<string, ExternalProviderOptions> ExternalProviders { get; set; }
    public LanguageOptions Language { get; set; }
    public UserOptions User { get; set; }
    public AppOptions App { get; set; }
    public PasswordOptions Password { get; set; }
    public ProtectionOptions Protection { get; set; }
    public bool TrackSessionActivity { get; set; } = true;

    public void ValidateAndThrowIfNeeded()
    {
        // Validation logic here
        if (User.LoginType == LoginType.Any && User.ConfirmRequired)
            throw new ApplicationException("No confirmation flow available when login is random string");

        if (!Language.UseCustomLanguages)
        {
            if (Language.Languages == null || Language.Languages.Length == 0)
                throw new ApplicationException("Should be at least one language");
        }
    }
}
