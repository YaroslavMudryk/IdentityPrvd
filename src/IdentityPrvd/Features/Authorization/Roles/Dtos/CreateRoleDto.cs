namespace IdentityPrvd.Features.Authorization.Roles.Dtos;

public class CreateRoleDto
{
    public string Name { get; set; }
    public bool IsDefault { get; set; }
    public string[] ClaimIds { get; set; } = [];
}
