namespace IdentityPrvd.Features.Authentication.ExternalSignin.Dtos;

public class ExternalUserDto
{
    public string ProviderUserId { get; set; }
    public string FullName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Language { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Picture { get; set; }
    public string Provider { get; set; }
    public string Uri { get; set; }
}
