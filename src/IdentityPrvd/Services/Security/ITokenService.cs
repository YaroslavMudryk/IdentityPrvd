using IdentityPrvd.Features.Shared.Dtos;

namespace IdentityPrvd.Services.Security;

public interface ITokenService
{
    Task<JwtToken> GetUserTokenAsync(Guid userId, string sessionId, string audience = null);
    Task<JwtToken> GetUserTokenAsync(Guid userId, string sessionId, string provider, string audience = null);
    Task<Dictionary<string, List<string>>> GetUserPermissionsAsync(Guid userId, string clientId);
}
