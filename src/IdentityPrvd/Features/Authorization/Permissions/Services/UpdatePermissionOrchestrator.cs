using FluentValidation;
using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Helpers;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Features.Authorization.Permissions.Dtos;

namespace IdentityPrvd.Features.Authorization.Permissions.Services;

public class UpdatePermissionOrchestrator(
    IIdentityContext identityContext,
    IValidator<UpdatePermissionDto> validator,
    IPermissionsQuery permissionsQuery,
    IPermissionStore permissionStore)
{
    public async Task<PermissionDto> UpdatePermissionAsync(Guid permissionId, UpdatePermissionDto dto)
    {
        var currentUser = identityContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionOrRoles(
             IdentityPermissions.Permissions.Manage,
             [DefaultsRoles.Admin]);

        dto.Id = permissionId;
        await ValidationHelper.ValidateAndThrowAsync(validator, dto);

        var permission = await permissionStore.GetAsync(permissionId);
        permission.Value = dto.Value;
        permission.DisplayName = dto.DisplayName;

        await permissionStore.UpdateAsync(permission);

        return await permissionsQuery.GetPermissionAsync(permissionId);
    }
}
