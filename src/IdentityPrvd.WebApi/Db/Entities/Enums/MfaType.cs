namespace IdentityPrvd.WebApi.Db.Entities.Enums;

public enum MfaType
{
    Totp = 1,
    Email,
    Phone,
    Passwordless
}
