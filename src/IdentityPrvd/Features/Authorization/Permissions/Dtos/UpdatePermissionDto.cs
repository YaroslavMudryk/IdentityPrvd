namespace IdentityPrvd.Features.Authorization.Permissions.Dtos;

public class UpdatePermissionDto
{
    public Guid Id { get; set; }
    public string Value { get; set; }
    public string DisplayName { get; set; }
}
