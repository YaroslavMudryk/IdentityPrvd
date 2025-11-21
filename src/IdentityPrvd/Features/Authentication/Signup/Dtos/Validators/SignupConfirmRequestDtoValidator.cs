using FluentValidation;
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
            .CustomAsync(async (code, context, token) =>
            {
                var confirm = await confirmsQuery.GetConfirmWithUserByCodeAsync(code);

                if (confirm == null)
                {
                    context.AddFailure("Confirm not found");
                    return;
                }

                if (confirm.User.IsConfirmed)
                {
                    context.AddFailure("User already confirmed");
                    return;
                }

                if (confirm.IsActivated)
                {
                    context.AddFailure("Confirm already activated");
                    return;
                }

                var utcNow = timeProvider.GetUtcNow().DateTime;

                if (confirm.ActiveFrom > utcNow || confirm.ActiveTo < utcNow)
                {
                    context.AddFailure("Verify out of time");
                    return;
                }
            });
    }
}
