using System.Security.Claims;

namespace IdentityPrvd.Services.Security;

public class TokenClaimsContext(Guid userId, string sessionId, List<Claim> claims, string? provider = null)
{
    public Guid UserId { get; } = userId;
    public string SessionId { get; } = sessionId;
    public string? Provider { get; } = provider;
    public List<Claim> Claims { get; } = claims;
}

public interface ITokenPermissionsContributor
{
    Task ContributeAsync(TokenClaimsContext context, CancellationToken cancellationToken = default);
}
