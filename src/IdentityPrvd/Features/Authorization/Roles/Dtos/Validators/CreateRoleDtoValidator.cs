using FluentValidation;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Data.Queries;

namespace IdentityPrvd.Features.Authorization.Roles.Dtos.Validators;

public class CreateRoleDtoValidator : AbstractValidator<CreateRoleDto>
{
    public CreateRoleDtoValidator(
        IClaimsQuery claimsQuery,
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

        RuleFor(s => s.ClaimIds)
            .MustAsync(async (claimIds, token) =>
            {
                if (claimIds != null && claimIds.Length != 0)
                {
                    var allClaimsExists = await claimsQuery.GetClaimsByIdsAsync(claimIds);
                    if (allClaimsExists.Count != claimIds.Length)
                        throw new BadRequestException("Some claims do not exist or are invalid");
                }

                return true;
            });
    }
}
