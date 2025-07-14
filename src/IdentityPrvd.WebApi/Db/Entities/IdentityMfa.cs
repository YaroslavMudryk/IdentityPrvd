using IdentityPrvd.WebApi.Db.Entities.Enums;

namespace IdentityPrvd.WebApi.Db.Entities;

public class IdentityMfa : BaseModel
{
    public DateTime? Activated { get; set; }
    public Ulid? ActivatedBySessionId { get; set; }
    public string Secret { get; set; }
    public MfaType Type { get; set; }
    public DateTime? DiactivedAt { get; set; }
    public Ulid? DiactivedBySessionId { get; set; }
    public Ulid UserId { get; set; }
    public IdentityUser User { get; set; }
    public List<IdentityMfaRecoveryCode> RecoveryCodes { get; set; } = [];
}
