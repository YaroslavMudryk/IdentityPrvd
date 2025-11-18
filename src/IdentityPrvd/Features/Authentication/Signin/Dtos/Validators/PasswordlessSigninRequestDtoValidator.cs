using FluentValidation;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Options;
using IdentityPrvd.Services.Security;
using System.Text.RegularExpressions;

namespace IdentityPrvd.Features.Authentication.Signin.Dtos.Validators;

public class PasswordlessSigninRequestDtoValidator : AbstractValidator<PasswordlessSigninRequestDto>
{
    public PasswordlessSigninRequestDtoValidator(
        IUsersQuery usersQuery,
        IClientsQuery clientsQuery,
        IHasher hasher,
        TimeProvider timeProvider,
        IdentityPrvdOptions options)
    {
        RuleFor(dto => dto)
            .Custom((_, _) =>
            {
                if (!options.Signin.Passwordless)
                    throw new BadRequestException("errors.signin.passwordless_disabled");
            });

        RuleFor(x => x.Login)
            .NotEmpty()
            .WithMessage("Login is required.")
            .Must((login) =>
            {
                if (options.User.LoginType == LoginType.Email)
                {
                    var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
                    if (!emailRegex.IsMatch(login))
                        throw new BadRequestException("Login must be a valid email address");
                }
                else if (options.User.LoginType == LoginType.Phone)
                {
                    var phoneRegex = new Regex(@"^\+?[1-9]\d{1,14}$");
                    if (!phoneRegex.IsMatch(login))
                        throw new BadRequestException("Login must be a valid phone number");
                }
                else if (options.User.LoginType == LoginType.Any)
                {
                    if (string.IsNullOrWhiteSpace(login))
                        throw new BadRequestException("Login is required");

                    if (login.Length < 4)
                        throw new BadRequestException("Login must be at least 4 characters long");
                }

                return true;
            })
            .WithMessage("Login format is invalid.");

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("OTP code is required.")
            .Must(code => code.Length >= 6 && code.Length <= 8 && code.All(char.IsDigit))
            .WithMessage("OTP code must be 6-8 digits.");

        RuleFor(x => x).MustAsync(async (dto, _) =>
        {
            var utcNow = timeProvider.GetUtcNow().UtcDateTime;
            var user = await usersQuery.GetUserByLoginNullableAsync(dto.Login)
                    ?? throw new BadRequestException("Login or OTP code is incorrect");

            if (!user.IsConfirmed)
                throw new BadRequestException("User is not confirmed");

            var client = await clientsQuery.GetClientByIdNullableAsync(dto.ClientId)
                    ?? throw new NotFoundException($"Client {dto.ClientId} not found");

            if (!client.IsActive)
                throw new BadRequestException("Client is not active");

            if (client.ActiveFrom > utcNow || client.ActiveTo.HasValue && client.ActiveTo.Value < utcNow)
                throw new BadRequestException("Client is not active at this time");

            if (client.ClientSecretRequired)
            {
                var clientSecret = await clientsQuery.GetClientSecretNullableAsync(client.Id.GetIdAsString())
                    ?? throw new NotFoundException($"Not found secret for clientId:{dto.ClientId}");

                if (!hasher.Verify(clientSecret.Value, dto.ClientSecret))
                    throw new BadRequestException("Secret is invalid");
            }

            return true;
        });
    }
}
