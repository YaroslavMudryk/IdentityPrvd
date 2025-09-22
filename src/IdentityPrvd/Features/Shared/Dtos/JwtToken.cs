namespace IdentityPrvd.Features.Shared.Dtos;

public class JwtToken
{
    public string Token { get; set; }
    public string SessionId { get; set; }
    public int ExpireIn { get; set; }
}
