namespace IdentityPrvd.Features.Authorization.Permissions.Dtos;

public class PermissionDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Type { get; set; }
    public string Value { get; set; }
    public string Issuer { get; set; }
    public string DisplayName { get; set; }
    public int RolesCount { get; set; }
    public int ClientsCount { get; set; }
    public DateTime UpdatedAt { get; set; }
}
