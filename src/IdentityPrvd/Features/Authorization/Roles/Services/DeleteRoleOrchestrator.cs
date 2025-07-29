using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Domain.Entities;

namespace IdentityPrvd.Features.Authorization.Roles.Services;

public class DeleteRoleOrchestrator(
    IRoleStore roleStore,
    IRoleClaimStore roleClaimStore,
    IRolesQuery rolesQuery,
    IUserContext userContext)
{
    public async Task DeleteRoleAsync(Ulid roleId)
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionsOrRoles(
             IdentityClaims.Types.Role, IdentityClaims.Values.Delete,
             [DefaultsRoles.SuperAdmin, DefaultsRoles.Admin]);

        //await using var transaction = await roleStore.BeginTransactionAsync();

        var roleToDelete = await roleStore.GetAsync(roleId);
        await EnsureThatRoleCanBeDeletedAsync(roleToDelete);
        await roleStore.DeleteAsync(roleToDelete);

        var roleClaimsToDelete = await roleClaimStore.GetRoleClaimsByRoleIdAsync(roleId);
        await roleClaimStore.DeleteRangeAsync(roleClaimsToDelete);

        //await transaction.CommitAsync();
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