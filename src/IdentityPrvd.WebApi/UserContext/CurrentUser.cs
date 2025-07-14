using IdentityPrvd.WebApi.Exceptions;
using IdentityPrvd.WebApi.Helpers;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace IdentityPrvd.WebApi.UserContext;

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

public record BasicAuthenticatedUser(string UserId, string SessionId, IEnumerable<Claim> Claims) : CurrentUser
{
    public void EnsureUserHasPermissions(string type, string value)
    {
        if (Claims.Any(s => s.Type == IdentityClaims.Types.Roles && s.Value.Contains(DefaultsRoles.SuperAdmin)))
            return;

        if (Claims.Where(s => s.Type == type).Where(s => s.Value == value).Any())
            return;

        throw new UnauthorizedException();
    }

    public void IsIsRoles(string[] roles)
    {
        if (Claims.Any(s => s.Type == IdentityClaims.Types.Roles && roles.Contains(s.Value)))
            return;

        throw new UnauthorizedException();
    }

    public void EnsureUserHasPermissionsOrRoles(string type, string value, string[] roles)
    {
        if (Claims.Any(s => s.Type == IdentityClaims.Types.Roles && s.Value.Contains(DefaultsRoles.SuperAdmin)))
            return;

        if (Claims.Any(s => s.Type == IdentityClaims.Types.Roles && roles.Contains(s.Value)))
            return;

        if (Claims.Where(s => s.Type == type).Where(s => s.Value == value).Any())
            return;

        throw new UnauthorizedException();
    }
}

public static class CurrentUserHelper
{
    public static CurrentUser GetCurrentUser(this ClaimsPrincipal user, Dictionary<string, List<string>> permissions)
    {
        ArgumentNullException.ThrowIfNull(user);

        var userIdClaim = user.Claims.FirstOrDefault(s => s.Type == IdentityClaims.Types.UserId);
        var sessionIdClaim = user.Claims.FirstOrDefault(s => s.Type == IdentityClaims.Types.SessionId);
        var otherClaims = user.Claims.Where(s => s.Type != IdentityClaims.Types.UserId && s.Type != IdentityClaims.Types.SessionId);

        if (userIdClaim is null || sessionIdClaim is null)
            return UnauthenticatedUser.Instance;

        return new BasicAuthenticatedUser(userIdClaim.Value,
            sessionIdClaim.Value,
            GetClaims(otherClaims, permissions));
    }

    private static IEnumerable<Claim> GetClaims(IEnumerable<Claim> basicClaims, Dictionary<string, List<string>> permissions)
    {
        foreach (var permission in permissions)
        {
            foreach (var value in permission.Value)
            {
                if (!basicClaims.Any(s => s.Type == permission.Key && s.Value == value))
                    basicClaims = basicClaims.Append(new Claim(permission.Key, value));
            }
        }

        return basicClaims;
    }
}
