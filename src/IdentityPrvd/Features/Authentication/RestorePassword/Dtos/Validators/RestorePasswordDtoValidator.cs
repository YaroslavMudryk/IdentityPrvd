using FluentValidation;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Options;
using System.Text.RegularExpressions;

namespace IdentityPrvd.Features.Authentication.RestorePassword.Dtos.Validators;

public class RestorePasswordDtoValidator : AbstractValidator<RestorePasswordDto>
{
    public RestorePasswordDtoValidator(IdentityPrvdOptions options)
    {
        RuleFor(x => x.Password)
            .Custom((password, context) =>
            {
                if (string.IsNullOrWhiteSpace(password))
                    context.AddFailure("password", "Password is required");

                if (password.Length < 6)
                    context.AddFailure("password", "Password must be at least 6 characters long");

                if (!string.IsNullOrEmpty(options.Password.Regex))
                {
                    var passwordRegex = new Regex(options.Password.Regex);
                    if (!passwordRegex.IsMatch(password))
                        throw new BadRequestException($"{options.Password.RegexErrorMessage}");
                }
            });
    }
}
