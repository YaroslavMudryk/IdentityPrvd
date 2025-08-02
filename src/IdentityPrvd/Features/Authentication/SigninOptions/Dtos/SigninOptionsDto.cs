namespace IdentityPrvd.Features.Authentication.SigninOptions.Dtos;

public class SigninOptionsDto
{
    public bool PasswordSignin { get; set; } = true;
    public List<string> ExternalProviders { get; set; } = [];
}
