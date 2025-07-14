namespace IdentityPrvd.WebApi.Features.Signin.Dtos;

public class SigninResponseDto
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public int ExpiredIn { get; set; }
}
