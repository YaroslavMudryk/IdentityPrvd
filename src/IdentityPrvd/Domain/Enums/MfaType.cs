namespace IdentityPrvd.Domain.Enums;

public enum MfaType
{
    Totp = 1,
    Email,
    Phone,
    Passwordless
}
