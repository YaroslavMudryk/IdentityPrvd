namespace IdentityPrvd.WebApi.Db.Entities;

public class IdentityRoleClaim : BaseModel
{
    public DateTime ActiveFrom { set; get; }
    public DateTime? ActiveTo { set; get; }
    public bool IsActive { get; set; }
    public Ulid RoleId { get; set; }
    public IdentityRole Role { get; set; }
    public Ulid ClaimId { get; set; }
    public IdentityClaim Claim { get; set; }
}
