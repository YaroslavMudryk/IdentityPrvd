using FluentValidation;
using IdentityPrvd.WebApi.Exceptions;
using IdentityPrvd.WebApi.Features.Signup.DataAccess;
using IdentityPrvd.WebApi.Options;
using System.Text.RegularExpressions;

namespace IdentityPrvd.WebApi.Features.Signup.Dtos.Validators;

public class SignupRequestDtoValidator : AbstractValidator<SignupRequestDto>
{
    public SignupRequestDtoValidator(
        IUserValidatorQuery validatorQuery,
        IdentityPrvdOptions options)
    {
        RuleFor(s => s)
            .MustAsync(async (dto, token) =>
        {
            if (options.UserOptions.LoginType == LoginType.Email)
            {
                var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
                if (!emailRegex.IsMatch(dto.Login))
                    throw new BadRequestException("Login must be a valid email address");
            }
            else if (options.UserOptions.LoginType == LoginType.Phone)
            {
                var phoneRegex = new Regex(@"^\+?[1-9]\d{1,14}$");
                if (!phoneRegex.IsMatch(dto.Login))
                    throw new BadRequestException("Login must be a valid phone number");
            }
            else if (options.UserOptions.LoginType == LoginType.Any)
            {
                if (string.IsNullOrWhiteSpace(dto.Login))
                    throw new BadRequestException("Login is required");
                
                if (dto.Login.Length < 5)
                    throw new BadRequestException("Login must be at least 5 characters long");
            }

            var isExistUser = await validatorQuery.IsExistUserByLoginAsync(dto.Login);
            if (isExistUser)
                throw new BadRequestException("User with this login already exists");

            isExistUser = await validatorQuery.IsExistUserByUserNameAsync(dto.UserName);
            if (isExistUser)
                throw new BadRequestException("User with this userName already exists");

            return true;
        });

        RuleFor(s => s.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .Must((dto, token) =>
            {
                if (!string.IsNullOrEmpty(options.PasswordOptions.Regex))
                {
                    var passwordRegex = new Regex(options.PasswordOptions.Regex);
                    if (!passwordRegex.IsMatch(dto.Password))
                        throw new BadRequestException($"{options.PasswordOptions.RegexErrorMessage}");
                }

                return true;
            });
    }
}
