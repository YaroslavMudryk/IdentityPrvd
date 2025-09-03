using FluentValidation;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Common.Helpers;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Options;
using IdentityPrvd.Services.Security;

namespace IdentityPrvd.Features.Authentication.Signin.Dtos.Validators;

public class SigninRequestDtoValidator : AbstractValidator<SigninRequestDto>
{
    public SigninRequestDtoValidator(
        IUsersQuery usersQuery,
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
            .EmailAddress()
            .WithMessage("Email must be a valid email address.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters long.");

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
                if (userHelper.IsBlocked(user))
                {
                    throw new BadRequestException("User is blocked until " + user.BlockedUntil!.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                }

                if (user.FailedLoginAttemptsCount >= 5)
                {
                    user.FailedLoginAttemptsCount = 0;
                    user.BlockedUntil = utcNow.AddHours(1);
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
}
