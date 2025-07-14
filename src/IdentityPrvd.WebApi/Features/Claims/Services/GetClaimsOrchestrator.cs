using IdentityPrvd.WebApi.Features.Claims.DataAccess;
using IdentityPrvd.WebApi.Features.Claims.Dtos;
using IdentityPrvd.WebApi.Helpers;
using IdentityPrvd.WebApi.UserContext;

namespace IdentityPrvd.WebApi.Features.Claims.Services;

public class GetClaimsOrchestrator(
    IUserContext userContext,
    ClaimsQuery query)
{
    public async Task<IReadOnlyList<ClaimDto>> GetClaimsAsync()
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionsOrRoles(
             IdentityClaims.Types.Claims, IdentityClaims.Values.ViewAll,
             [DefaultsRoles.SuperAdmin, DefaultsRoles.Admin]);

        return await query.GetClaimsAsync();
    }
}
