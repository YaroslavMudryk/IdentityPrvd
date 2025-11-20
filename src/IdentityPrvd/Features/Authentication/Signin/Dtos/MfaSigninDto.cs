namespace IdentityPrvd.Features.Authentication.Signin.Dtos;

public class MfaSigninDto
{
    public string VerificationId { get; set; }
    public string Code { get; set; }
}
