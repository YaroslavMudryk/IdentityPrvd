using FluentValidation;
using IdentityPrvd.WebApi.Exceptions;
using IdentityPrvd.WebApi.Features.Claims.DataAccess;

namespace IdentityPrvd.WebApi.Features.Claims.Dtos.Validators;

public class CreateClaimDtoValidator : AbstractValidator<CreateClaimDto>
{
    public CreateClaimDtoValidator(IClaimsValidatorQuery claimsValidatorQuery)
    {
        RuleFor(x => x.Type)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Value)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Issuer)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x)
            .MustAsync(async (dto, token) =>
            {
                var existingClaim = await claimsValidatorQuery.GetClaimByTypeAndValueAsync(dto.Type, dto.Value);
                if (existingClaim != null)
                    throw new BadRequestException("Claim with this type and value is already exsits");

                return true;
            });
    }
}
