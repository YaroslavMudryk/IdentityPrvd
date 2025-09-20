using FluentValidation;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Data.Queries;

namespace IdentityPrvd.Features.Authorization.Claims.Dtos.Validators;

public class UpdateClaimDtoValidator : AbstractValidator<UpdateClaimDto>
{
    public UpdateClaimDtoValidator(IClaimsQuery claimsQuery)
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
                var claimByTypeAndValue = await claimsQuery.GetClaimByTypeAndValueAsync(dto.Type, dto.Value);

                if (claimByTypeAndValue is null)
                    return true;

                if (claimByTypeAndValue.Id != dto.Id)
                    throw new BadRequestException("Claim with the same type and value already exists");

                return true;
            });
    }
}
