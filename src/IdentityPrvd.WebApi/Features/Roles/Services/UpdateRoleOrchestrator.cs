using FluentValidation;
using IdentityPrvd.WebApi.Db.Entities;
using IdentityPrvd.WebApi.Features.Roles.DataAccess;
using IdentityPrvd.WebApi.Features.Roles.Dtos;
using IdentityPrvd.WebApi.Helpers;
using IdentityPrvd.WebApi.UserContext;

namespace IdentityPrvd.WebApi.Features.Roles.Services;

public class UpdateRoleOrchestrator(
    IUserContext userContext,
    IValidator<UpdateRoleDto> validator,
    RolesQuery query,
    RoleRepo roleRepo,
    DefaultRoleService defaultRoleService,
    RoleClaimRepo roleClaimRepo)
{
    public async Task<RoleDto> UpdateRoleAsync(Ulid roleId, UpdateRoleDto dto)
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionsOrRoles(
             IdentityClaims.Types.Role, IdentityClaims.Values.Update,
             [DefaultsRoles.SuperAdmin, DefaultsRoles.Admin]);

        await validator.ValidateAndThrowAsync(dto);

        await using var transaction = await roleRepo.BeginTransactionAsync();

        var roleToUpdate = await roleRepo.GetAsync(roleId);
        roleToUpdate.Name = dto.Name;
        roleToUpdate.NameNormalized = dto.Name.ToUpper();
        roleToUpdate.IsDefault = false;
        await roleRepo.UpdateAsync(roleToUpdate);

        if (dto.IsDefault)
            await defaultRoleService.MakeRoleAsDefaultAsync(roleId);

        await UpdateRoleClaimsAsync(roleId, dto.ClaimIds);

        await transaction.CommitAsync();

        return await query.GetRoleAsync(roleId);
    }

    private async Task UpdateRoleClaimsAsync(Ulid roleId, Ulid[] newClaimIds)
    {
        var roleClaimsToDelete = await roleClaimRepo.GetRoleClaimsByRoleIdAsync(roleId);
        await roleClaimRepo.DeleteRangeAsync(roleClaimsToDelete);

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
            await roleClaimRepo.AddRangeAsync(roleClaims);
        }
    }
}
