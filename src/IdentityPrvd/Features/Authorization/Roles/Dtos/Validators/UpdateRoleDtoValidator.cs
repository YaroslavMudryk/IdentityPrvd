using FluentValidation;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Data.Queries;

namespace IdentityPrvd.Features.Authorization.Roles.Dtos.Validators;

public class UpdateRoleDtoValidator : AbstractValidator<UpdateRoleDto>
{
    public UpdateRoleDtoValidator(
        IPermissionsQuery permissionsQuery,
        IRolesQuery rolesQuery)
    {
        RuleFor(s => s)
            .MustAsync(async (dto, token) =>
            {
                var roleByName = await rolesQuery.GetRoleByNameAsync(dto.Name.ToUpper());

                if (roleByName is null)
                    return true;

                if (roleByName.Id != dto.Id)
                    throw new BadRequestException("Role with the same name is already exist");

                if (dto.PermissionIds != null && dto.PermissionIds.Length != 0)
                {
                    var allPermissionsExists = await permissionsQuery.GetPermissionsByIdsAsync([.. dto.PermissionIds.Select(s=>s.GetIdAsGuid())]);
                    if (allPermissionsExists.Count != dto.PermissionIds.Length)
                        throw new BadRequestException("Some permissions do not exist or are invalid");
                }

                if (!dto.IsDefault)
                {
                    var currentRole = await rolesQuery.GetRoleByIdAsync(dto.Id);
                    if (currentRole.IsDefault)
                        throw new BadRequestException("Default role cannot be updated to non-default role");
                }

                return true;
            });
    }
}
