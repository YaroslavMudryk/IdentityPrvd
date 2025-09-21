using IdentityPrvd.Features.Shared.Dtos;

namespace IdentityPrvd.Services.Security;

public interface ITokenService
{
    Task<JwtToken> GetUserTokenAsync(Ulid userId, string sessionId, string audience = null);
    Task<JwtToken> GetUserTokenAsync(Ulid userId, string sessionId, string provider, string audience = null);
    Task<Dictionary<string, List<string>>> GetUserPermissionsAsync(Ulid userId, string clientId);
}
