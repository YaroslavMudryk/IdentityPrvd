namespace IdentityPrvd.WebApi.Features.LinkExternalSignin.Dtos;

public class ExternalSigninDto
{
    public string Provider { get; set; }
    public string Picture { get; set; }
    public bool IsLinked { get; set; }
    public DateTime? LinkedAt { get; set; }
}
