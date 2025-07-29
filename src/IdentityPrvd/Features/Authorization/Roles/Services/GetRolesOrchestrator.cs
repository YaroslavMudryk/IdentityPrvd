using IdentityPrvd.Common.Constants;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Features.Authorization.Roles.Dtos;

namespace IdentityPrvd.Features.Authorization.Roles.Services;

public class GetRolesOrchestrator(
    IUserContext userContext,
    IRolesQuery query)
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
