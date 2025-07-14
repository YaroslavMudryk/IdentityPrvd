using IdentityPrvd.WebApi.Db.Entities.Enums;

namespace IdentityPrvd.WebApi.Db.Entities;

public class IdentityContact : BaseModel
{
    public string Title { get; set; }
    public string Value { get; set; }
    public ContactType Type { get; set; }
    public bool IsMain { get; set; }
    public bool IsConfirmed { get; set; }
    public DateTime? ConfirmedAt { get; set; }

    public Ulid UserId { get; set; }
    public IdentityUser User { get; set; }
}
