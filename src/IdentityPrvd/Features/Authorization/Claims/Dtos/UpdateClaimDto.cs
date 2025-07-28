namespace IdentityPrvd.Features.Authorization.Claims.Dtos;

public class UpdateClaimDto
{
    public Ulid Id { get; set; }
    public string Type { get; set; }
    public string Value { get; set; }
    public string Issuer { get; set; }
    public string DisplayName { get; set; }
}
