using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityPrvd.WebApi.Db.Entities;

public class IdentityBan : BaseModel
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string Cause { get; set; }
    [NotMapped]
    public bool IsPermanent => End == DateTime.MaxValue;
    public Ulid UserId { get; set; }
    public IdentityUser User { get; set; }
}
