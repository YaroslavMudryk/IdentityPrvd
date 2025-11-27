namespace IdentityPrvd.Domain.Entities;

public class IdentityClientPermission : BaseModel
{
    public DateTime ActiveFrom { set; get; }
    public DateTime? ActiveTo { set; get; }
    public bool IsActive { get; set; }
    public Guid PermissionId { get; set; }
    public IdentityPermission Permission { get; set; }
    public Guid ClientId { get; set; }
    public IdentityClient Client { get; set; }
}
