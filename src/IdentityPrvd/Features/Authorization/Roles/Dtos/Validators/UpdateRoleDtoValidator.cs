using FluentValidation;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Data.Queries;

namespace IdentityPrvd.Features.Authorization.Roles.Dtos.Validators;

public class UpdateRoleDtoValidator : AbstractValidator<UpdateRoleDto>
{
    public UpdateRoleDtoValidator(
        IClaimsQuery claimsQuery,
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

                if (dto.ClaimIds != null && dto.ClaimIds.Length != 0)
                {
                    var allClaimsExists = await claimsQuery.GetClaimsByIdsAsync([.. dto.ClaimIds.Select(s=>s.GetIdAsUlid())]);
                    if (allClaimsExists.Count != dto.ClaimIds.Length)
                        throw new BadRequestException("Some claims do not exist or are invalid");
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
