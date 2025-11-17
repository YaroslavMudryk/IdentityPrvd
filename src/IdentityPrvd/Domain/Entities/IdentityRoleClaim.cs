namespace IdentityPrvd.Domain.Entities;

public class IdentityRoleClaim : BaseModel
{
    public DateTime ActiveFrom { set; get; }
    public DateTime? ActiveTo { set; get; }
    public bool IsActive { get; set; }
    public Guid RoleId { get; set; }
    public IdentityRole Role { get; set; }
    public Guid ClaimId { get; set; }
    public IdentityClaim Claim { get; set; }
}
