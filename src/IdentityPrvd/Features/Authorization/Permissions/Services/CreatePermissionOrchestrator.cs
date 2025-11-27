using FluentValidation;
using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Helpers;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Features.Authorization.Permissions.Dtos;

namespace IdentityPrvd.Features.Authorization.Permissions.Services;

public class CreatePermissionOrchestrator(
    IIdentityContext identityContext,
    IValidator<CreatePermissionDto> validator,
    IPermissionsQuery query,
    IPermissionStore permissionStore)
{
    public async Task<PermissionDto> CreatePermissionAsync(CreatePermissionDto dto)
    {
        var currentUser = identityContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionOrRoles(
             IdentityPermissions.Permissions.Manage,
             [DefaultsRoles.Admin]);

        await ValidationHelper.ValidateAndThrowAsync(validator, dto);

        var newPermission = new IdentityPermission
        {
            Value = dto.Value,
            DisplayName = dto.DisplayName
        };
        await permissionStore.AddAsync(newPermission);

        return await query.GetPermissionAsync(newPermission.Id);
    }
}
