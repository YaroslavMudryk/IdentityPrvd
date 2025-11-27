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

public class CreateRoleOrchestrator(
    IIdentityContext identityContext,
    IRolesQuery query,
    IRoleStore roleRepo,
    ITransactionManager transactionManager,
    DefaultRoleService defaultRoleService,
    IRolePermissionStore rolePermissionRepo,
    IValidator<CreateRoleDto> validator)
{
    public async Task<RoleDto> CreateRoleAsync(CreateRoleDto dto)
    {
        var currentUser = identityContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionOrRoles(
             IdentityPermissions.Roles.Manage,
             [DefaultsRoles.Admin]);

        await ValidationHelper.ValidateAndThrowAsync(validator, dto);
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

        var newRolePermissions = dto.PermissionIds.Select(permissionId => new IdentityRolePermission
        {
            Id = Guid.CreateVersion7(),
            RoleId = newRole.Id,
            PermissionId = permissionId.GetIdAsGuid(),
            ActiveFrom = DateTime.MinValue,
            ActiveTo = DateTime.MaxValue,
            IsActive = true
        });
        await rolePermissionRepo.AddRangeAsync(newRolePermissions);
        await transaction.CommitAsync();

        return await query.GetRoleAsync(newRole.Id);
    }
}
