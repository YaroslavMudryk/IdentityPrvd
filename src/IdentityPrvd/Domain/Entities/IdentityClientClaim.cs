namespace IdentityPrvd.Domain.Entities;

public class IdentityClientClaim : BaseModel
{
    public DateTime ActiveFrom { set; get; }
    public DateTime? ActiveTo { set; get; }
    public bool IsActive { get; set; }
    public Guid ClaimId { get; set; }
    public IdentityClaim Claim { get; set; }
    public Guid ClientId { get; set; }
    public IdentityClient Client { get; set; }
}
