using IdentityPrvd.WebApi.Db.Entities.Internal;

namespace IdentityPrvd.WebApi.Features.Signin.Dtos;

public class SigninRequestDto
{
    public string Login { get; set; }
    public string Password { get; set; }
    public string Language { get; set; } = "uk";
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string AppVersion { get; set; }
    public ClientInfo Client { get; set; }
}
