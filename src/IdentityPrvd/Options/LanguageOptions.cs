namespace IdentityPrvd.Options;

public class LanguageOptions
{
    public bool LanguageRequired { get; set; } = true;
    public bool UseCustomLanguages { get; set; } = false;
    public string[] Languages { get; set; } = ["en", "ua"];

    public void EnsureEnglishLanguage()
    {
        var comparer = StringComparer.OrdinalIgnoreCase;

        if (Languages == null || Languages.Length == 0)
        {
            Languages = ["en"];
            return;
        }

        if (!Languages.Any(language => comparer.Equals(language, "en")))
        {
            Languages = [.. Languages
                .Concat(["en"])
                .Distinct(comparer)
                .Select(language => comparer.Equals(language, "en") ? "en" : language)];
        }
        else
        {
            // Normalize casing to "en"
            Languages = [.. Languages.Select(language => comparer.Equals(language, "en") ? "en" : language)];
        }
    }
}
