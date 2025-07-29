using FluentValidation;
using IdentityPrvd.Common.Constants;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Data.Transactions;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Features.Authorization.Roles.Dtos;

namespace IdentityPrvd.Features.Authorization.Roles.Services;

public class UpdateRoleOrchestrator(
    IUserContext userContext,
    IValidator<UpdateRoleDto> validator,
    IRolesQuery query,
    IRoleStore roleStore,
    ITransactionManager transactionManager,
    DefaultRoleService defaultRoleService,
    IRoleClaimStore roleClaimStore)
{
    public async Task<RoleDto> UpdateRoleAsync(Ulid roleId, UpdateRoleDto dto)
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionsOrRoles(
             IdentityClaims.Types.Role, IdentityClaims.Values.Update,
             [DefaultsRoles.SuperAdmin, DefaultsRoles.Admin]);

        await validator.ValidateAndThrowAsync(dto);

        await using var transaction = await transactionManager.BeginTransactionAsync();

        var roleToUpdate = await roleStore.GetAsync(roleId);
        roleToUpdate.Name = dto.Name;
        roleToUpdate.NameNormalized = dto.Name.ToUpper();
        roleToUpdate.IsDefault = false;
        await roleStore.UpdateAsync(roleToUpdate);

        if (dto.IsDefault)
            await defaultRoleService.MakeRoleAsDefaultAsync(roleId);

        await UpdateRoleClaimsAsync(roleId, dto.ClaimIds);

        await transaction.CommitAsync();

        return await query.GetRoleAsync(roleId);
    }

    private async Task UpdateRoleClaimsAsync(Ulid roleId, Ulid[] newClaimIds)
    {
        var roleClaimsToDelete = await roleClaimStore.GetRoleClaimsByRoleIdAsync(roleId);
        await roleClaimStore.DeleteRangeAsync(roleClaimsToDelete);

        if (newClaimIds != null && newClaimIds.Any())
        {
            var roleClaims = newClaimIds.Select(roleClaimId => new IdentityRoleClaim
            {
                RoleId = roleId,
                ClaimId = roleClaimId,
                ActiveFrom = DateTime.MinValue,
                ActiveTo = DateTime.MaxValue,
                IsActive = true
            });
            await roleClaimStore.AddRangeAsync(roleClaims);
        }
    }
}
