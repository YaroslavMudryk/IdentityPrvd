namespace IdentityPrvd.Domain.Entities;

public class IdentityClient : BaseModel
{
    public string Name { get; set; }
    public string ShortName { get; set; }
    public string Description { get; set; }
    public string Image { get; set; }
    public string ClientId { get; set; }
    public bool ClientSecretRequired { get; set; }
    public bool IsActive { get; set; }
    public DateTime ActiveFrom { set; get; }
    public DateTime? ActiveTo { set; get; }
    public string[] RedirectUris { get; set; } = ["*"];
    public List<IdentityClientClaim> ClientClaims { get; set; } = [];
    public List<IdentityClientSecret> ClientSecrets { get; set; } = [];
}
