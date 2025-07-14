using IdentityPrvd.WebApi.Db.Entities.Enums;

namespace IdentityPrvd.WebApi.Db.Entities;

public class IdentityConfirm : BaseModel
{
    public string VerifyId { get; set; }
    public string Code { get; set; }
    public DateTime ActiveFrom { get; set; }
    public DateTime ActiveTo { get; set; }
    public bool IsActivated { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public ConfirmType Type { get; set; }
    public Ulid? ContactId { get; set; }
    public Ulid UserId { get; set; }
    public IdentityUser User { get; set; }
}
