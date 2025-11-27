using FluentValidation;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Data.Queries;

namespace IdentityPrvd.Features.Authorization.Permissions.Dtos.Validators;

public class CreatePermissionDtoValidator : AbstractValidator<CreatePermissionDto>
{
    public CreatePermissionDtoValidator(IPermissionsQuery permissionsQuery)
    {
        RuleFor(x => x.Value)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x)
            .MustAsync(async (dto, token) =>
            {
                var existingPermission = await permissionsQuery.GetPermissionByValueAsync(dto.Value);
                if (existingPermission != null)
                    throw new BadRequestException("Permission with this type and value is already exsits");

                return true;
            });
    }
}
