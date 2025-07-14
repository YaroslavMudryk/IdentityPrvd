namespace IdentityPrvd.WebApi.Db.Entities;

public class IdentityClaim : BaseModel
{
    public string Type { get; set; }
    public string Value { get; set; }
    public string Issuer { get; set; }
    public string DisplayName { get; set; }
    public List<IdentityRoleClaim> RoleClaims { get; set; } = [];
    public List<IdentityClientClaim> ClientClaims { get; set; } = [];
}
