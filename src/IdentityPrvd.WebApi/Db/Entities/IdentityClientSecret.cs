namespace IdentityPrvd.WebApi.Db.Entities;

public class IdentityClientSecret : BaseModel
{
    public string Value { get; set; }
    public bool IsActive { get; set; }
    public Ulid ClientId { get; set; }
    public IdentityClient Client { get; set; }
}
