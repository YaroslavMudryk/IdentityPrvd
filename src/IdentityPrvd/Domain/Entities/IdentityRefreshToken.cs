namespace IdentityPrvd.Domain.Entities;

public class IdentityRefreshToken : BaseModel
{
    public string Value { get; set; }
    public DateTime? UsedAt { set; get; }
    public DateTime ExpiredAt { set; get; }
    public Guid SessionId { set; get; }
    public IdentitySession Session { set; get; }
}
