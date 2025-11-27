using IdentityPrvd.Common.Constants;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Data.Transactions;

namespace IdentityPrvd.Features.Authorization.Permissions.Services;

public class DeletePermissionOrchestrator(
    IIdentityContext identityContext,
    ITransactionManager transactionManager,
    IPermissionStore permissionStore)
{
    public async Task DeletePermissionAsync(Guid permissionId)
    {
        var currentUser = identityContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionOrRoles(
             IdentityPermissions.Permissions.Manage,
             [DefaultsRoles.Admin]);

        await using var transaction = await transactionManager.BeginTransactionAsync();

        await DeletePermissionReferencesAsync(permissionId);
        var permission = await permissionStore.GetAsync(permissionId);
        await permissionStore.DeleteAsync(permission);

        await transaction.CommitAsync();
    }

    private async Task DeletePermissionReferencesAsync(Guid permissionId)
    {
        var clientPermissionsToDelete = await permissionStore.GetClientPermissionsByIdAsync(permissionId);
        await permissionStore.DeleteClientPermissionsAsync(clientPermissionsToDelete);

        var rolePermissionsToDelete = await permissionStore.GetRolePermissionsByIdAsync(permissionId);
        await permissionStore.DeleteRolePermissionsAsync(rolePermissionsToDelete);
    }
}
