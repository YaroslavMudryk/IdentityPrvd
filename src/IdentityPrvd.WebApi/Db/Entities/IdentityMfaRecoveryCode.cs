namespace IdentityPrvd.WebApi.Db.Entities;

public class IdentityMfaRecoveryCode : BaseModel
{
    public string CodeHash { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime ExpiryAt { get; set; }
    public Ulid MfaId { get; set; }
    public IdentityMfa Mfa { get; set; }
}
