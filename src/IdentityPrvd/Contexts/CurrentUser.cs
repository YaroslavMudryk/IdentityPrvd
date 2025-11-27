using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Exceptions;
using System.Security.Claims;

namespace IdentityPrvd.Contexts;

public abstract record CurrentUser;

public record UninitializedUser : CurrentUser
{
    public static UninitializedUser Instance { get; } = new();
}

public record UnauthenticatedUser : CurrentUser
{
    public static UnauthenticatedUser Instance { get; } = new();
}

public record ServiceUser(string System) : CurrentUser
{
    public static ServiceUser Instance { get; } = new("Api");
}

public record BasicAuthenticatedUser(string UserId, string SessionId, IEnumerable<Claim> Claims, IReadOnlyList<string> Permissions) : CurrentUser
{
    public bool IsInRoles(string[] roles)
    {
        return Claims.Any(s => s.Type == IdentityClaims.Types.Roles && roles.Contains(s.Value));
    }

    public void EnsureUserHasPermission(string permission)
    {
        if (Permissions.Any(s => s.Contains(permission)))
            return;

        throw new UnauthorizedException();
    }

    public void EnsureUserHasPermissionOrRoles(string permission, string[] roles)
    {
        if (Claims.Any(s => s.Type == IdentityClaims.Types.Roles && roles.Contains(s.Value)))
            return;

        if (Permissions.Any(s => s.Contains(permission)))
            return;

        throw new UnauthorizedException();
    }
}

public static class CurrentUserHelper
{
    public static CurrentUser GetCurrentUser(this ClaimsPrincipal user, IReadOnlyList<string> permissions)
    {
        ArgumentNullException.ThrowIfNull(user);

        var userIdClaim = user.Claims.FirstOrDefault(s => s.Type == IdentityClaims.Types.UserId);
        var sessionIdClaim = user.Claims.FirstOrDefault(s => s.Type == IdentityClaims.Types.SessionId);
        var otherClaims = user.Claims.Where(s => s.Type != IdentityClaims.Types.UserId && s.Type != IdentityClaims.Types.SessionId);

        if (userIdClaim is null || sessionIdClaim is null)
            return UnauthenticatedUser.Instance;

        return new BasicAuthenticatedUser(userIdClaim.Value,
            sessionIdClaim.Value,
            otherClaims, permissions);
    }
}
