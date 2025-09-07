using IdentityPrvd.Domain.Enums;

namespace IdentityPrvd.Domain.Entities;

public class IdentityCode : BaseModel
{
    public string VerifyId { get; set; }
    public string Code { get; set; }
    public DateTime ActiveFrom { get; set; }
    public DateTime ActiveTo { get; set; }
    public bool IsActivated { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public CodeType Type { get; set; }
    public Ulid? ContactId { get; set; }
    public Ulid UserId { get; set; }
    public IdentityUser User { get; set; }
}
