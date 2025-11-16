namespace IdentityPrvd.Features.Authentication.Signin.Dtos;

public class SigninUserOptionsDto
{
    public bool Password { get; set; }
    public bool Passwordless { get; set; }
    public List<string> LinkedExternalProviders { get; set; } = [];
}
