namespace IdentityPrvd.Options;

public static class IdentityPrvdOptionsValidation
{
    public static void ValidateAndThrowIfNeeded(this IdentityPrvdOptions options)
    {
        if (options.User.LoginType == LoginType.Any && options.User.ConfirmRequired)
            throw new ApplicationException("No confirmation flow available when login is random string");

        if (!options.Language.UseCustomLanguages)
        {
            if (options.Language.Languages == null || options.Language.Languages.Length == 0)
                throw new ApplicationException("Should be at least one language");
        }
    }
}
