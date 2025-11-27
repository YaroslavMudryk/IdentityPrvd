using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Data.Transactions;
using IdentityPrvd.Domain.Entities;

namespace IdentityPrvd.Features.Authorization.Roles.Services;

public class DeleteRoleOrchestrator(
    IRoleStore roleStore,
    IRolePermissionStore rolePermissionStore,
    IRolesQuery rolesQuery,
    ITransactionManager transactionManager,
    IIdentityContext identityContext)
{
    public async Task DeleteRoleAsync(Guid roleId)
    {
        var currentUser = identityContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionOrRoles(
             IdentityPermissions.Roles.Manage,
             [DefaultsRoles.Admin]);

        await using var transaction = await transactionManager.BeginTransactionAsync();

        var roleToDelete = await roleStore.GetAsync(roleId);
        await EnsureThatRoleCanBeDeletedAsync(roleToDelete);
        await roleStore.DeleteAsync(roleToDelete);

        var rolePermissionsToDelete = await rolePermissionStore.GetRolePermissionsByRoleIdAsync(roleId);
        await rolePermissionStore.DeleteRangeAsync(rolePermissionsToDelete);

        await transaction.CommitAsync();
    }

    public async Task EnsureThatRoleCanBeDeletedAsync(IdentityRole role)
    {
        if (role.IsDefault)
            throw new BadRequestException("Default role can't be deleted");

        var countUsersAssignedToRole = await rolesQuery.GetUsersCountByRoleIdAsync(role.Id);
        if (countUsersAssignedToRole > 0)
            throw new BadRequestException($"Role '{role.Name}' can't be deleted because it is assigned to {countUsersAssignedToRole} users");
    }
}
