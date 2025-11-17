namespace IdentityPrvd.Domain.Entities;

public class IdentityClientSecret : BaseModel
{
    public string Value { get; set; }
    public bool IsActive { get; set; }
    public Guid ClientId { get; set; }
    public IdentityClient Client { get; set; }
}
