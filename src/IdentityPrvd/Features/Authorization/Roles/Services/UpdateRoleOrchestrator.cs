using FluentValidation;
using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Common.Helpers;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Data.Transactions;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Features.Authorization.Roles.Dtos;

namespace IdentityPrvd.Features.Authorization.Roles.Services;

public class UpdateRoleOrchestrator(
    IIdentityContext identityContext,
    IValidator<UpdateRoleDto> validator,
    IRolesQuery query,
    IRoleStore roleStore,
    ITransactionManager transactionManager,
    DefaultRoleService defaultRoleService,
    IRolePermissionStore rolePermissionStore)
{
    public async Task<RoleDto> UpdateRoleAsync(Guid roleId, UpdateRoleDto dto)
    {
        var currentUser = identityContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionOrRoles(
             IdentityPermissions.Roles.Manage,
             [DefaultsRoles.Admin]);

        dto.Id = roleId;
        await ValidationHelper.ValidateAndThrowAsync(validator, dto);

        await using var transaction = await transactionManager.BeginTransactionAsync();

        var roleToUpdate = await roleStore.GetAsync(roleId);
        roleToUpdate.Name = dto.Name;
        roleToUpdate.NameNormalized = dto.Name.ToUpper();
        roleToUpdate.IsDefault = false;
        await roleStore.UpdateAsync(roleToUpdate);

        if (dto.IsDefault)
            await defaultRoleService.MakeRoleAsDefaultAsync(roleId);

        await UpdateRolePermissionsAsync(roleId, [.. dto.PermissionIds.Select(s => s.GetIdAsGuid())]);

        await transaction.CommitAsync();

        return await query.GetRoleAsync(roleId);
    }

    private async Task UpdateRolePermissionsAsync(Guid roleId, Guid[] newPermissionIds)
    {
        var rolePermissionsToDelete = await rolePermissionStore.GetRolePermissionsByRoleIdAsync(roleId);
        await rolePermissionStore.DeleteRangeAsync(rolePermissionsToDelete);

        if (newPermissionIds != null && newPermissionIds.Length != 0)
        {
            var rolePermissions = newPermissionIds.Select(rolePermissionId => new IdentityRolePermission
            {
                RoleId = roleId,
                PermissionId = rolePermissionId,
                ActiveFrom = DateTime.MinValue,
                ActiveTo = DateTime.MaxValue,
                IsActive = true
            });
            await rolePermissionStore.AddRangeAsync(rolePermissions);
        }
    }
}
