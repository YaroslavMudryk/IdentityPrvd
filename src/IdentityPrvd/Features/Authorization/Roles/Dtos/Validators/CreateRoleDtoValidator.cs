using FluentValidation;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Data.Queries;

namespace IdentityPrvd.Features.Authorization.Roles.Dtos.Validators;

public class CreateRoleDtoValidator : AbstractValidator<CreateRoleDto>
{
    public CreateRoleDtoValidator(
        IPermissionsQuery permissionsQuery,
        IRolesQuery rolesQuery)
    {
        RuleFor(s => s.Name)
            .NotEmpty().WithMessage("Can't be empty")
            .MustAsync(async (name, token) =>
            {
                var existingRole = await rolesQuery.GetRoleByNameAsync(name.ToUpper());
                if (existingRole != null)
                    throw new BadRequestException("Role with the same name is already exist");

                return true;
            });

        RuleFor(s => s.PermissionIds)
            .MustAsync(async (permissionIds, token) =>
            {
                if (permissionIds != null && permissionIds.Length != 0)
                {
                    var allPermissionsExists = await permissionsQuery.GetPermissionsByIdsAsync([.. permissionIds.Select(s => s.GetIdAsGuid())]);
                    if (allPermissionsExists.Count != permissionIds.Length)
                        throw new BadRequestException("Some permissions do not exist or are invalid");
                }

                return true;
            });
    }
}
