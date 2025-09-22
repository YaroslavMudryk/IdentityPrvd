namespace IdentityPrvd.Features.Shared.Dtos;

public class SigninResponseDto
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public int ExpireIn { get; set; }
    public bool RequiredMfa { get; set; }
    public string VerifyId { get; set; }
}
