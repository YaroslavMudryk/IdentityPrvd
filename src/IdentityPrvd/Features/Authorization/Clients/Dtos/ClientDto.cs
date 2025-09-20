namespace IdentityPrvd.Features.Authorization.Clients.Dtos;

public class ClientDto
{
    public Ulid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Name { get; set; }
    public string ShortName { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string Description { get; set; }
    public string Image { get; set; }
    public bool IsActive { get; set; }
    public DateTime ActiveFrom { set; get; }
    public DateTime? ActiveTo { set; get; }
    public IReadOnlyList<string> RedirectUris { get; set; } = ["*"];
    public DateTime UpdatedAt { get; set; }
    public IReadOnlyList<string> ClaimsIds { get; set; } = [];
}
