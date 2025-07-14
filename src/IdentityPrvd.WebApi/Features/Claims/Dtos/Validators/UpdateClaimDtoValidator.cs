using FluentValidation;
using IdentityPrvd.WebApi.Exceptions;
using IdentityPrvd.WebApi.Features.Claims.DataAccess;

namespace IdentityPrvd.WebApi.Features.Claims.Dtos.Validators;

public class UpdateClaimDtoValidator : AbstractValidator<UpdateClaimDto>
{
    public UpdateClaimDtoValidator(IClaimsValidatorQuery claimsValidatorQuery)
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
                var claimByTypeAndValue = await claimsValidatorQuery.GetClaimByTypeAndValueAsync(dto.Type, dto.Value) ??
                    throw new NotFoundException($"Claim with id:{dto.Id} not found");

                if (claimByTypeAndValue.Id != dto.Id)
                    throw new BadRequestException("Claim with the same type and value already exists");

                return true;
            });
    }
}
