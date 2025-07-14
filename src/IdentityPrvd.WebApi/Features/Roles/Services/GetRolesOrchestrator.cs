using IdentityPrvd.WebApi.Features.Roles.DataAccess;
using IdentityPrvd.WebApi.Features.Roles.Dtos;
using IdentityPrvd.WebApi.Helpers;
using IdentityPrvd.WebApi.UserContext;

namespace IdentityPrvd.WebApi.Features.Roles.Services;

public class GetRolesOrchestrator(
    IUserContext userContext,
    RolesQuery query)
{
    public async Task<IReadOnlyList<RoleDto>> GetRolesAsync()
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionsOrRoles(
             IdentityClaims.Types.Role, IdentityClaims.Values.ViewAll,
             [DefaultsRoles.SuperAdmin, DefaultsRoles.Admin]);

        return await query.GetRolesAsync();
    }
}
