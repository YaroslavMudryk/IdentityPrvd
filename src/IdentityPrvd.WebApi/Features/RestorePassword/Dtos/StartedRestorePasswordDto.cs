namespace IdentityPrvd.WebApi.Features.RestorePassword.Dtos;

public class StartedRestorePasswordDto
{
    public string Login { get; set; }
    public string Code { get; set; }
    public string VerifyId { get; set; }
}
