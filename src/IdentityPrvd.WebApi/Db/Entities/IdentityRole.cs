namespace IdentityPrvd.WebApi.Db.Entities;

public class IdentityRole : BaseModel
{
    public string Name { get; set; }
    public bool IsDefault { get; set; }
    public string NameNormalized { get; set; }
    public List<IdentityUserRole> UserRoles { get; set; } = [];
    public List<IdentityRoleClaim> RoleClaims { get; set; } = [];
}
