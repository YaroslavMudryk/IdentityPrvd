using FluentValidation;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Common.Helpers;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Options;
using IdentityPrvd.Services.Security;
using System.Text.RegularExpressions;

namespace IdentityPrvd.Features.Authentication.Signin.Dtos.Validators;

public class SigninRequestDtoValidator : AbstractValidator<SigninRequestDto>
{
    public SigninRequestDtoValidator(
        IUsersQuery usersQuery,
        IUserStore userStore,
        IClientsQuery clientsQuery,
        IHasher hasher,
        IUserSecureService userSecureService,
        TimeProvider timeProvider,
        IdentityPrvdOptions options,
        UserHelper userHelper)
    {
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
            .WithMessage("Email must be a valid email address.");

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

        RuleFor(x => x.Language)
            .Must((language) =>
            {
                if (options.Language.LanguageRequired)
                {
                    if (!options.Language.UseCustomLanguages)
                    {
                        if (!options.Language.Languages.Any(s => s.Contains(language)))
                            throw new BadRequestException($"Language `{language}` is not at available list ({string.Join(',', options.Language.Languages)})");
                    }
                }

                return true;
            });

        RuleFor(x => x).MustAsync(async (dto, _) =>
        {
            var utcNow = timeProvider.GetUtcNow().UtcDateTime;
            var user = await usersQuery.GetUserByLoginNullableAsync(dto.Login)
                    ?? throw new BadRequestException("Login or password is incorrect");

            if (!user.IsConfirmed)
                throw new BadRequestException("User is not confirmed");

            if (user.CanBeBlocked)
            {
                await CheckBlockUserStatusAsync(userStore, timeProvider, user);

                if (user.FailedLoginAttemptsCount >= 5)
                {
                    await userSecureService.IncrementFailedLoginByBlockAsync(user, utcNow);
                    throw new BadRequestException("User is blocked until " + user.BlockedUntil!.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                }
            }

            if (!hasher.Verify(user.PasswordHash, dto.Password))
            {
                await userSecureService.IncrementFailedLoginByPasswordAsync(user, utcNow);
                throw new BadRequestException("Login or password is incorrect");
            }

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


    private static async Task CheckBlockUserStatusAsync(IUserStore userStore, TimeProvider timeProvider, IdentityUser identityUser)
    {
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        if (identityUser.BlockedUntil == null)
            return;

        if (identityUser.BlockedUntil.HasValue)
        {
            if (identityUser.BlockedUntil.Value > utcNow)
                throw new BadRequestException("User is blocked until " + identityUser.BlockedUntil!.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            else
            {
                identityUser.BlockedUntil = null;
                await userStore.UpdateAsync(identityUser, true);
            }
        }
    }
}
