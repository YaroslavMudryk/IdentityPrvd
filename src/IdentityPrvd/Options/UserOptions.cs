namespace IdentityPrvd.Options;

public class UserOptions
{
    public LoginType LoginType { get; set; } = LoginType.Email;
    public bool ConfirmRequired { get; set; } = false;
    public int ConfirmCodeValidInMinutes { get; set; } = 5;

    public bool VerifyPasswordOnChangeLogin { get; set; } = false;
    public bool UseOldPasswords { get; set; } = true;
    public bool ForceSignoutEverywhere { get; set; } = false;
}

public enum LoginType
{
    Email = 1,
    Phone,
    Any
}
