using IdentityPrvd.Domain.Entities;

namespace IdentityPrvd.Common.Extensions;

public static class MfaRecoveryCodeExtensions
{
    public static string GetMaskedCode(string recoveryCode)
    {
        return $"****-{recoveryCode[^4..]}";
    }

    public static TimeSpan GetRemainingValidity(this IdentityMfaRecoveryCode recoveryCode, TimeProvider timeProvider)
    {
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        return recoveryCode.ExpiryAt - utcNow;
    }
}
