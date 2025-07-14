using FluentValidation;
using IdentityPrvd.WebApi.Db.Entities;
using IdentityPrvd.WebApi.Features.Roles.DataAccess;
using IdentityPrvd.WebApi.Features.Roles.Dtos;
using IdentityPrvd.WebApi.Helpers;
using IdentityPrvd.WebApi.UserContext;

namespace IdentityPrvd.WebApi.Features.Roles.Services;

public class CreateRoleOrchestrator(
    IUserContext userContext,
    RolesQuery query,
    RoleRepo roleRepo,
    DefaultRoleService defaultRoleService,
    RoleClaimRepo roleClaimRepo,
    IValidator<CreateRoleDto> validator)
{
    public async Task<RoleDto> CreateRoleAsync(CreateRoleDto dto)
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionsOrRoles(
             IdentityClaims.Types.Role, IdentityClaims.Values.Create,
             [DefaultsRoles.SuperAdmin, DefaultsRoles.Admin]);

        await validator.ValidateAndThrowAsync(dto);
        await using var transaction = await roleRepo.BeginTransactionAsync();
        var newRole = new IdentityRole
        {
            Name = dto.Name,
            NameNormalized = dto.Name.ToUpper(),
            IsDefault = false,
        };
        await roleRepo.AddAsync(newRole);

        if (dto.IsDefault)
            await defaultRoleService.MakeRoleAsDefaultAsync(newRole.Id);

        var newRoleClaims = dto.ClaimIds.Select(claimId => new IdentityRoleClaim
        {
            RoleId = newRole.Id,
            ClaimId = claimId,
            ActiveFrom = DateTime.MinValue,
            ActiveTo = DateTime.MaxValue,
            IsActive = true
        });
        await roleClaimRepo.AddRangeAsync(newRoleClaims);
        await transaction.CommitAsync();

        return await query.GetRoleAsync(newRole.Id);
    }
}
