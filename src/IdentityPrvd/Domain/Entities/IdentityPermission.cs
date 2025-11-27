namespace IdentityPrvd.Domain.Entities;

public class IdentityPermission : BaseModel
{
    public string Value { get; set; }
    public string DisplayName { get; set; }
    public List<IdentityRolePermission> RolePermissions { get; set; } = [];
    public List<IdentityClientPermission> ClientPermissions { get; set; } = [];
}
