namespace IdentityPrvd.Features.Authorization.Roles.Dtos;

public class RoleDto
{
    public Ulid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Name { get; set; }
    public bool IsDefault { get; set; }
    public string NameNormalized { get; set; }
    public int UsersCount { get; set; }
    public int ClaimsCount { get; set; }
    public DateTime UpdatedAt { get; set; }
}
