namespace IdentityPrvd.WebApi.Options;

public class IdentityPrvdOptions
{
    public bool TrackSessionActivity { get; set; } = true;
    public UserOptions UserOptions { get; set; } = new UserOptions();
    public PasswordOptions PasswordOptions { get; set; } = new PasswordOptions();
    public LanguageOptions LanguageOptions { get; set; } = new LanguageOptions();
}

public class UserOptions
{
    public LoginType LoginType { get; set; } = LoginType.Email;
    public bool ConfirmRequired { get; set; } = false;
    public int ConfirmCodeValidInMinutes { get; set; } = 5;

    public bool VerifyPasswordOnChangeLogin { get; set; } = false;
    public bool UseOldPasswords { get; set; } = true;
    public bool ForceSignoutEverywhere { get; set; } = false;
}

public class PasswordOptions
{
    public string Regex { get; set; } = string.Empty;
    public string RegexErrorMessage = string.Empty;
    public bool AllowDigits { get; set; } = false;
    public int MinLength { get; set; } = 6;
    public int MaxLength { get; set; } = 100;
}

public class LanguageOptions
{
    public bool LanguageRequired { get; set; } = true;
    public bool UseCustomLanguages { get; set; } = false;
    public string[] Languages { get; set; } = ["en", "ua"];
}

public enum LoginType
{
    Email = 1,
    Phone,
    Any
}
