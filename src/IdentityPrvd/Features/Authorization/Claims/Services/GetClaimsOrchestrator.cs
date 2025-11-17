using IdentityPrvd.Common.Constants;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Features.Authorization.Claims.Dtos;

namespace IdentityPrvd.Features.Authorization.Claims.Services;

public class GetClaimsOrchestrator(
    IIdentityContext identityContext,
    IClaimsQuery query)
{
    public async Task<IReadOnlyList<ClaimDto>> GetClaimsAsync()
    {
        var currentUser = identityContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionsOrRoles(
             IdentityClaims.Types.Claims, IdentityClaims.Values.ViewAll,
             [DefaultsRoles.SuperAdmin, DefaultsRoles.Admin]);

        return await query.GetClaimsAsync();
    }
}
