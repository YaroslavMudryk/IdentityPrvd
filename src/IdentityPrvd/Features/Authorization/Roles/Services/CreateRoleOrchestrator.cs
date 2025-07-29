using FluentValidation;
using IdentityPrvd.Common.Constants;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Data.Transactions;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Features.Authorization.Roles.Dtos;

namespace IdentityPrvd.Features.Authorization.Roles.Services;

public class CreateRoleOrchestrator(
    IUserContext userContext,
    IRolesQuery query,
    IRoleStore roleRepo,
    ITransactionManager transactionManager,
    DefaultRoleService defaultRoleService,
    IRoleClaimStore roleClaimRepo,
    IValidator<CreateRoleDto> validator)
{
    public async Task<RoleDto> CreateRoleAsync(CreateRoleDto dto)
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionsOrRoles(
             IdentityClaims.Types.Role, IdentityClaims.Values.Create,
             [DefaultsRoles.SuperAdmin, DefaultsRoles.Admin]);

        await validator.ValidateAndThrowAsync(dto);
        await using var transaction = await transactionManager.BeginTransactionAsync();
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
