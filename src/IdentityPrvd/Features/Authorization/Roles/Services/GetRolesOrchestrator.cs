using IdentityPrvd.Common.Constants;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Features.Authorization.Roles.Dtos;

namespace IdentityPrvd.Features.Authorization.Roles.Services;

public class GetRolesOrchestrator(
    IIdentityContext identityContext,
    IRolesQuery query)
{
    public async Task<IReadOnlyList<RoleDto>> GetRolesAsync()
    {
        var currentUser = identityContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionOrRoles(
             IdentityPermissions.Roles.Read,
             [DefaultsRoles.Admin]);

        return await query.GetRolesAsync();
    }
}
