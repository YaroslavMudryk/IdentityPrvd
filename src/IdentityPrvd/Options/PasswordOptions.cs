namespace IdentityPrvd.Options;

public class PasswordOptions
{
    public string Regex { get; set; } = string.Empty;
    public string RegexErrorMessage = string.Empty;
    public bool AllowDigits { get; set; } = false;
    public int MinLength { get; set; } = 6;
    public int MaxLength { get; set; } = 100;
}
