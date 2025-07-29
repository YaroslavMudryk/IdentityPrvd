namespace IdentityPrvd.Features.Authentication.RestorePassword.Dtos;

public class RestorePasswordDto
{
    public string Hint { get; set; }
    public string Password { get; set; }
    public string Code { get; set; }
    public string VerifyId { get; set; }
}
