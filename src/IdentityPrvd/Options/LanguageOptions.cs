namespace IdentityPrvd.Options;

public class LanguageOptions
{
    public bool LanguageRequired { get; set; } = true;
    public bool UseCustomLanguages { get; set; } = false;
    public string[] Languages { get; set; } = ["en", "ua"];
}
