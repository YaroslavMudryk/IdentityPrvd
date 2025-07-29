using IdentityPrvd.Domain.ValueObjects;

namespace IdentityPrvd.Features.Authentication.Signin.Dtos;

public class SigninRequestDto
{
    public string Login { get; set; }
    public string Password { get; set; }
    public string Language { get; set; } = "en";
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string AppVersion { get; set; }
    public Dictionary<string, string> Data { get; set; }
    public ClientInfo Client { get; set; }
}
