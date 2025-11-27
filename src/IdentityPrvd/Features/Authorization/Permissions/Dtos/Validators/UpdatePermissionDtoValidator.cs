using FluentValidation;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Data.Queries;

namespace IdentityPrvd.Features.Authorization.Permissions.Dtos.Validators;

public class UpdatePermissionDtoValidator : AbstractValidator<UpdatePermissionDto>
{
    public UpdatePermissionDtoValidator(IPermissionsQuery permissionsQuery)
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
                var permissionByTypeAndValue = await permissionsQuery.GetPermissionByValueAsync(dto.Value);

                if (permissionByTypeAndValue is null)
                    return true;

                if (permissionByTypeAndValue.Id != dto.Id)
                    throw new BadRequestException("Permission with the same type and value already exists");

                return true;
            });
    }
}
