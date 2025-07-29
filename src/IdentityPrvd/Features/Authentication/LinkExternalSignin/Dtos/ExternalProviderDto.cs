namespace IdentityPrvd.Features.Authentication.LinkExternalSignin.Dtos;

public class ExternalProviderDto
{
    public string Provider { get; set; }
    public string Picture { get; set; }
    public bool IsLinked { get; set; }
    public DateTime? LinkedAt { get; set; }
}
