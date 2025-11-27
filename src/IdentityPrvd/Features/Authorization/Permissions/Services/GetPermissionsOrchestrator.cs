using IdentityPrvd.Common.Constants;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Features.Authorization.Permissions.Dtos;

namespace IdentityPrvd.Features.Authorization.Permissions.Services;

public class GetPermissionsOrchestrator(
    IIdentityContext identityContext,
    IPermissionsQuery permissionsQuery)
{
    public async Task<IReadOnlyList<PermissionDto>> GetPermissionsAsync()
    {
        var currentUser = identityContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionOrRoles(
             IdentityPermissions.Permissions.Read,
             [DefaultsRoles.Admin]);

        return await permissionsQuery.GetPermissionsAsync();
    }
}
