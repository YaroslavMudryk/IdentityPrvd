namespace IdentityPrvd.Domain.Entities;

public class IdentityUserRole : BaseModel
{
    public DateTime ActiveFrom { set; get; }
    public DateTime? ActiveTo { set; get; }
    public bool IsActive { get; set; }
    public Ulid UserId { get; set; }
    public IdentityUser User { get; set; }
    public Ulid RoleId { get; set; }
    public IdentityRole Role { get; set; }
}
