namespace IdentityPrvd.Features.Security.Initialize.Dtos;

public class InitializeResponseDto
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }

    public string Login { get; set; }
    public string Password { get; set; }
}
