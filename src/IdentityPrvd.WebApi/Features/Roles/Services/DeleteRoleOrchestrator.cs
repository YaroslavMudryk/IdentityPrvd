using IdentityPrvd.WebApi.Db.Entities;
using IdentityPrvd.WebApi.Exceptions;
using IdentityPrvd.WebApi.Features.Roles.DataAccess;
using IdentityPrvd.WebApi.Helpers;
using IdentityPrvd.WebApi.UserContext;

namespace IdentityPrvd.WebApi.Features.Roles.Services;

public class DeleteRoleOrchestrator(
    RoleRepo roleRepo,
    RoleClaimRepo roleClaimRepo,
    IRolesValidatorQuery rolesValidatorQuery,
    IUserContext userContext)
{
    public async Task DeleteRoleAsync(Ulid roleId)
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionsOrRoles(
             IdentityClaims.Types.Role, IdentityClaims.Values.Delete,
             [DefaultsRoles.SuperAdmin, DefaultsRoles.Admin]);

        await using var transaction = await roleRepo.BeginTransactionAsync();

        var roleToDelete = await roleRepo.GetAsync(roleId);
        await EnsureThatRoleCanBeDeletedAsync(roleToDelete);
        await roleRepo.DeleteAsync(roleToDelete);

        var roleClaimsToDelete = await roleClaimRepo.GetRoleClaimsByRoleIdAsync(roleId);
        await roleClaimRepo.DeleteRangeAsync(roleClaimsToDelete);

        await transaction.CommitAsync();
    }

    public async Task EnsureThatRoleCanBeDeletedAsync(IdentityRole role)
    {
        if (role.IsDefault)
            throw new BadRequestException("Default role can't be deleted");

        var countUsersAssignedToRole = await rolesValidatorQuery.GetUsersCountAssignedToRoleAsync(role.Id);
        if (countUsersAssignedToRole > 0)
            throw new BadRequestException($"Role '{role.Name}' can't be deleted because it is assigned to {countUsersAssignedToRole} users");
    }
}