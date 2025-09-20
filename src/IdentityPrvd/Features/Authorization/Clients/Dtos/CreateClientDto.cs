namespace IdentityPrvd.Features.Authorization.Clients.Dtos;

public class CreateClientDto
{
    public string Name { get; set; }
    public string ShortName { get; set; }
    public string ClientId { get; set; }
    public bool ClientSecretRequired { get; set; }
    public string Description { get; set; }
    public string Image { get; set; }
    public bool IsActive { get; set; }
    public DateTime ActiveFrom { set; get; }
    public DateTime? ActiveTo { set; get; }
    public string[] RedirectUris { get; set; } = ["*"];
}
