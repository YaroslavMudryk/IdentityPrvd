using FluentValidation;
using IdentityPrvd.WebApi.Exceptions;
using IdentityPrvd.WebApi.Features.Roles.DataAccess;

namespace IdentityPrvd.WebApi.Features.Roles.Dtos.Validators;

public class UpdateRoleDtoValidator : AbstractValidator<UpdateRoleDto>
{
    public UpdateRoleDtoValidator(IRolesValidatorQuery rolesValidatorQuery)
    {
        RuleFor(s => s)
            .MustAsync(async (dto, token) =>
            {
                var roleByName = await rolesValidatorQuery.GetRoleByNameAsync(dto.Name.ToUpper()) ??
                    throw new NotFoundException($"Role with id:{dto.Id} not found");

                if (roleByName.Id != dto.Id)
                    throw new BadRequestException("Role with the same name is already exist");

                if (dto.ClaimIds != null && dto.ClaimIds.Length != 0)
                {
                    var allClaimsExists = await rolesValidatorQuery.GetClaimsByIdsAsync(dto.ClaimIds);
                    if (allClaimsExists.Count != dto.ClaimIds.Length)
                        throw new BadRequestException("Some claims do not exist or are invalid");
                }

                if (!dto.IsDefault)
                {
                    var currentRole = await rolesValidatorQuery.GetRoleByIdAsync(dto.Id);
                    if (currentRole.IsDefault)
                        throw new BadRequestException("Default role cannot be updated to non-default role");
                }

                return true;
            });
    }
}
