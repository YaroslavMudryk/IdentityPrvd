namespace IdentityPrvd.Features.Authentication.Signin.Dtos;

public class SigninMfaRequestDto
{
    public string VerificationId { get; set; }
    public string Code { get; set; }
}
