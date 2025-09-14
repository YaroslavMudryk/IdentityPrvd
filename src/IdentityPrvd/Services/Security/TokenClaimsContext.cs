using System.Security.Claims;

namespace IdentityPrvd.Services.Security;

public class TokenClaimsContext(Ulid userId, string sessionId, List<Claim> claims, string? provider = null)
{
    public Ulid UserId { get; } = userId;
    public string SessionId { get; } = sessionId;
    public string? Provider { get; } = provider;
    public List<Claim> Claims { get; } = claims;
}

public interface ITokenClaimsContributor
{
    Task ContributeAsync(TokenClaimsContext context, CancellationToken cancellationToken = default);
}
