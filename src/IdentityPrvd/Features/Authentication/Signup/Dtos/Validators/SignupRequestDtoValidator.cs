using FluentValidation;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Options;
using System.Text.RegularExpressions;

namespace IdentityPrvd.Features.Authentication.Signup.Dtos.Validators;

public class SignupRequestDtoValidator : AbstractValidator<SignupRequestDto>
{
    public SignupRequestDtoValidator(
        IUsersQuery usersQuery,
        IdentityPrvdOptions options)
    {
        RuleFor(s => s)
            .MustAsync(async (dto, token) =>
            {
                if (options.User.LoginType == LoginType.Email)
                {
                    var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
                    if (!emailRegex.IsMatch(dto.Login))
                        throw new BadRequestException("Login must be a valid email address");
                }
                else if (options.User.LoginType == LoginType.Phone)
                {
                    var phoneRegex = new Regex(@"^\+?[1-9]\d{1,14}$");
                    if (!phoneRegex.IsMatch(dto.Login))
                        throw new BadRequestException("Login must be a valid phone number");
                }
                else if (options.User.LoginType == LoginType.Any)
                {
                    if (string.IsNullOrWhiteSpace(dto.Login))
                        throw new BadRequestException("Login is required");
                
                    if (dto.Login.Length < 4)
                        throw new BadRequestException("Login must be at least 4 characters long");
                }

                var isExistUser = await usersQuery.IsExistUserByLoginAsync(dto.Login);
                if (isExistUser)
                    throw new BadRequestException("User with this login already exists");

                isExistUser = await usersQuery.IsExistUserByUserNameAsync(dto.UserName);
                if (isExistUser)
                    throw new BadRequestException("User with this userName already exists");

                return true;
            });

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
