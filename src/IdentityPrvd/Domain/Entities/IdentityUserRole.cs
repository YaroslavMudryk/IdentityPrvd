namespace IdentityPrvd.Domain.Entities;

public class IdentityUserRole : BaseModel
{
    public DateTime ActiveFrom { set; get; }
    public DateTime? ActiveTo { set; get; }
    public bool IsActive { get; set; }
    public Guid UserId { get; set; }
    public IdentityUser User { get; set; }
    public Guid RoleId { get; set; }
    public IdentityRole Role { get; set; }
}
