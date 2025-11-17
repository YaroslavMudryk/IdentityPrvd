namespace IdentityPrvd.Services.Localization;

public interface ILocalizationService
{
    string GetString(string key, params object[] args);
    string GetString(string key, string language, params object[] args);
}
