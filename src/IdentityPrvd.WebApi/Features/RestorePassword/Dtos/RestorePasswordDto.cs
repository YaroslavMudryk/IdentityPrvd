namespace IdentityPrvd.WebApi.Features.RestorePassword.Dtos;

public class RestorePasswordDto
{
    public string Hint { get; set; }
    public string Password { get; set; }
    public string Code { get; set; }
    public string VerifyId { get; set; }
}
