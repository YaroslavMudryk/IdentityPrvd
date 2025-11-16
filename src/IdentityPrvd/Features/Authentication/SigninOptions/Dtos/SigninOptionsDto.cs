namespace IdentityPrvd.Features.Authentication.SigninOptions.Dtos;

public class SigninOptionsDto
{
    public bool Password { get; set; } = true;
    public bool Passwordless { get; set; } = true;
    public List<string> ExternalProviders { get; set; } = [];
}
