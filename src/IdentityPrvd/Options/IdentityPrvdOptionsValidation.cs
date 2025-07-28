namespace IdentityPrvd.Options;

public static class IdentityPrvdOptionsValidation
{
    public static void ValidateAndThrowIfNeeded(this IdentityPrvdOptions options)
    {
        if (options.UserOptions.LoginType == LoginType.Any && options.UserOptions.ConfirmRequired)
            throw new ApplicationException("No confirmation flow available when login is random string");

        if (!options.LanguageOptions.UseCustomLanguages)
        {
            if (options.LanguageOptions.Languages == null || options.LanguageOptions.Languages.Length == 0)
                throw new ApplicationException("Should be at least one language");
        }
    }
}
