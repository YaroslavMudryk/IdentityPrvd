using IdentityPrvd.Domain.Enums;

namespace IdentityPrvd.Domain.Entities;

public class IdentityMfa : BaseModel
{
    public DateTime? Activated { get; set; }
    public Guid? ActivatedBySessionId { get; set; }
    public string Secret { get; set; }
    public MfaType Type { get; set; }
    public DateTime? DiactivedAt { get; set; }
    public Guid? DiactivedBySessionId { get; set; }
    public Guid UserId { get; set; }
    public IdentityUser User { get; set; }
    public List<IdentityMfaRecoveryCode> RecoveryCodes { get; set; } = [];
}
