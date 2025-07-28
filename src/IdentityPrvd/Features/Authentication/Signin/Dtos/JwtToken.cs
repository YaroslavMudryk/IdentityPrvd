namespace IdentityPrvd.Features.Authentication.Signin.Dtos;

public class JwtToken
{
    public string Token { get; set; }
    public string SessionId { get; set; }
    public DateTime ExpiredAt { get; set; }
}
