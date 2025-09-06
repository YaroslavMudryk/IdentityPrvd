using FluentValidation;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Data.Queries;

namespace IdentityPrvd.Features.Authentication.Signup.Dtos.Validators;

public class SignupConfirmRequestDtoValidator : AbstractValidator<SignupConfirmRequestDto>
{
    public SignupConfirmRequestDtoValidator(
        TimeProvider timeProvider,
        IConfirmsQuery confirmsQuery)
    {
        RuleFor(s => s.Code)
            .NotEmpty()
            .WithMessage("Code is required.")
            .MustAsync(async(code, token) =>
            {
                var confirm = await confirmsQuery.GetConfirmWithUserByCodeAsync(code)
                    ?? throw new NotFoundException("Confirm not found");

                if (confirm.User.IsConfirmed)
                    throw new BadRequestException("User already confirmed");

                if (confirm.IsActivated)
                    throw new BadRequestException("Confirm already activated");

                var utcNow = timeProvider.GetUtcNow().UtcDateTime;
                if (confirm.ActiveFrom > utcNow || confirm.ActiveTo < utcNow)
                    throw new BadRequestException("Verify out of time");

                return true;
            });
    }
}
