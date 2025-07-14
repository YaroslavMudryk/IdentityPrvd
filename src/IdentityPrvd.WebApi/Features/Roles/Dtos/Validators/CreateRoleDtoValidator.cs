using FluentValidation;
using IdentityPrvd.WebApi.Exceptions;
using IdentityPrvd.WebApi.Features.Roles.DataAccess;

namespace IdentityPrvd.WebApi.Features.Roles.Dtos.Validators;

public class CreateRoleDtoValidator : AbstractValidator<CreateRoleDto>
{
    public CreateRoleDtoValidator(IRolesValidatorQuery rolesValidatorQuery)
    {
        RuleFor(s => s.Name)
            .NotEmpty().WithMessage("Can't be empty")
            .MustAsync(async (name, token) =>
            {
                var existingRole = await rolesValidatorQuery.GetRoleByNameAsync(name.ToUpper());
                if (existingRole != null)
                    throw new BadRequestException("Role with the same name is already exist");

                return true;
            });

        RuleFor(s => s.ClaimIds)
            .MustAsync(async (claimIds, token) =>
            {
                if (claimIds != null && claimIds.Length != 0)
                {
                    var allClaimsExists = await rolesValidatorQuery.GetClaimsByIdsAsync(claimIds);
                    if (allClaimsExists.Count != claimIds.Length)
                        throw new BadRequestException("Some claims do not exist or are invalid");
                }

                return true;
            });
    }
}
