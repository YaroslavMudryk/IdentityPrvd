using IdentityPrvd.Features.Authentication.Signin.Dtos;

namespace IdentityPrvd.Services.Security;

public interface ITokenService
{
    Task<JwtToken> GetUserTokenAsync(Ulid userId, string sessionId);
    Task<JwtToken> GetUserTokenAsync(Ulid userId, string sessionId, string provider);
    Task<Dictionary<string, List<string>>> GetUserPermissionsAsync(Ulid userId, string clientId);
}
