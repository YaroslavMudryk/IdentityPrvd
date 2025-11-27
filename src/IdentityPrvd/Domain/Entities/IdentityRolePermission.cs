namespace IdentityPrvd.Domain.Entities;

public class IdentityRolePermission : BaseModel
{
    public DateTime ActiveFrom { set; get; }
    public DateTime? ActiveTo { set; get; }
    public bool IsActive { get; set; }
    public Guid RoleId { get; set; }
    public IdentityRole Role { get; set; }
    public Guid PermissionId { get; set; }
    public IdentityPermission Permission { get; set; }
}
