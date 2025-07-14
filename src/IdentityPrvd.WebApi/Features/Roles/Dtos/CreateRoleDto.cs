namespace IdentityPrvd.WebApi.Features.Roles.Dtos;

public class CreateRoleDto
{
    public string Name { get; set; }
    public bool IsDefault { get; set; }
    public Ulid[] ClaimIds { get; set; } = [];
}
