using FluentValidation;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Common.Helpers;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Options;
using IdentityPrvd.Services.Localization;
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
        UserHelper userHelper,
        IFailedLoginAttemptsQuery failedLoginAttemptsQuery,
        IBansQuery bansQuery,
        ILocalizationService localizationService,
        IIdentityContext identityContext)
    {
        RuleFor(dto => dto)
            .Custom((_, _) =>
            {
                if (!options.Signin.Password)
                    throw new BadRequestException("errors.signin.password_disabled");
            });

        RuleFor(x => x.Login)
            .NotEmpty()
            .WithMessage((dto) => localizationService.GetString("validation.login.required", identityContext.CurrentLanguage))
            .Must((login) =>
            {
                var language = identityContext.CurrentLanguage ?? "en";
                if (options.User.LoginType == LoginType.Email)
                {
                    var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
                    if (!emailRegex.IsMatch(login))
                        throw new BadRequestException("validation.login.email_invalid");
                }
                else if (options.User.LoginType == LoginType.Phone)
                {
                    var phoneRegex = new Regex(@"^\+?[1-9]\d{1,14}$");
                    if (!phoneRegex.IsMatch(login))
                        throw new BadRequestException("validation.login.phone_invalid");
                }
                else if (options.User.LoginType == LoginType.Any)
                {
                    if (string.IsNullOrWhiteSpace(login))
                        throw new BadRequestException("validation.login.required");

                    if (login.Length < 4)
                        throw new BadRequestException("validation.login.min_length");
                }

                return true;
            });

        RuleFor(x => x.Password)
            .Custom((password, context) =>
            {
                var language = identityContext.CurrentLanguage ?? "en";
                if (string.IsNullOrWhiteSpace(password))
                    context.AddFailure("password", localizationService.GetString("validation.password.required", language));

                if (password.Length < 6)
                    context.AddFailure("password", localizationService.GetString("validation.password.min_length", language));

                if (!string.IsNullOrEmpty(options.Password.Regex))
                {
                    var passwordRegex = new Regex(options.Password.Regex);
                    if (!passwordRegex.IsMatch(password))
                    {
                        var errorMessage = !string.IsNullOrEmpty(options.Password.RegexErrorMessage)
                            ? options.Password.RegexErrorMessage
                            : localizationService.GetString("validation.password.regex", language, options.Password.RegexErrorMessage ?? "");
                        throw new BadRequestException("validation.password.regex", errorMessage);
                    }
                }
            });

        RuleFor(x => x.Language)
            .Must((language) =>
            {
                var currentLanguage = identityContext.CurrentLanguage ?? "en";
                if (options.Language.LanguageRequired)
                {
                    if (!options.Language.UseCustomLanguages)
                    {
                        if (!options.Language.Languages.Any(s => s.Contains(language)))
                            throw new BadRequestException("validation.language.invalid", language, string.Join(',', options.Language.Languages));
                    }
                }

                return true;
            });

        RuleFor(x => x).MustAsync(async (dto, _) =>
        {
            var utcNow = timeProvider.GetUtcNow().UtcDateTime;
            
            // Check rate limiting before user lookup to prevent user enumeration
            // Note: We can't ban here because we don't know the user yet, but we can block the attempt
            if (options.Protection.RateLimit.Enabled)
            {
                var timeWindowStart = utcNow.AddMinutes(-options.Protection.RateLimit.TimeWindowInMinutes);
                var recentAttempts = await failedLoginAttemptsQuery.CountFailedAttemptsByLoginAsync(dto.Login, timeWindowStart);
                
                if (recentAttempts >= options.Protection.RateLimit.MaxAttempts)
                {
                    var timeRemaining = options.Protection.RateLimit.TimeWindowInMinutes;
                    throw new BadRequestException("errors.rate_limit.exceeded", timeRemaining);
                }
            }
            
            var user = await usersQuery.GetUserByLoginNullableAsync(dto.Login)
                    ?? throw new BadRequestException("errors.login.incorrect");

            if (!user.IsConfirmed)
                throw new BadRequestException("errors.user.not_confirmed");

            // Check for active ban before allowing signin
            var activeBan = await bansQuery.GetActiveBanByUserIdAsync(user.Id, utcNow);
            if (activeBan != null)
            {
                throw new BadRequestException("errors.account.banned", activeBan.End.ToString("yyyy-MM-dd HH:mm:ss"), activeBan.Cause);
            }

            // Additional rate limiting check by user ID (in case user exists)
            if (options.Protection.RateLimit.Enabled)
            {
                var timeWindowStart = utcNow.AddMinutes(-options.Protection.RateLimit.TimeWindowInMinutes);
                var recentAttemptsByUser = await failedLoginAttemptsQuery.CountFailedAttemptsByUserIdAsync(user.Id, timeWindowStart);
                
                if (recentAttemptsByUser >= options.Protection.RateLimit.MaxAttempts)
                {
                    // Ban user due to rate limiting
                    if (user.CanBeBlocked)
                    {
                        await userSecureService.BanUserDueToRateLimitAsync(
                            user, 
                            utcNow, 
                            options.Protection.RateLimit.BanDurationInMinutes,
                            recentAttemptsByUser);
                    }
                    
                    var timeRemaining = options.Protection.RateLimit.BanDurationInMinutes;
                    throw new BadRequestException("errors.rate_limit.banned", timeRemaining);
                }
            }

            if (user.CanBeBlocked)
            {
                await CheckBlockUserStatusAsync(userStore, timeProvider, user);

                if (user.FailedLoginAttemptsCount >= 5)
                {
                    await userSecureService.IncrementFailedLoginByBlockAsync(user, utcNow);
                    throw new BadRequestException("errors.user.blocked", user.BlockedUntil!.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                }
            }

            if (!hasher.Verify(user.PasswordHash, dto.Password))
            {
                await userSecureService.IncrementFailedLoginByPasswordAsync(user, utcNow);
                throw new BadRequestException("errors.login.incorrect");
            }

            var client = await clientsQuery.GetClientByIdNullableAsync(dto.ClientId)
                    ?? throw new NotFoundException("errors.client.not_found", dto.ClientId);

            if (!client.IsActive)
                throw new BadRequestException("errors.client.not_active");

            if (client.ActiveFrom > utcNow || client.ActiveTo.HasValue && client.ActiveTo.Value < utcNow)
                throw new BadRequestException("errors.client.not_active_time");

            if (client.ClientSecretRequired)
            {
                var clientSecret = await clientsQuery.GetClientSecretNullableAsync(client.Id.GetIdAsString())
                    ?? throw new NotFoundException("errors.client.secret_not_found", dto.ClientId);

                if (!hasher.Verify(clientSecret.Value, dto.ClientSecret))
                    throw new BadRequestException("errors.client.secret_invalid");
            }

            return true;
        });
    }


    private async Task CheckBlockUserStatusAsync(IUserStore userStore, TimeProvider timeProvider, IdentityUser identityUser)
    {
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        if (identityUser.BlockedUntil == null)
            return;

        if (identityUser.BlockedUntil.HasValue)
        {
            if (identityUser.BlockedUntil.Value > utcNow)
                throw new BadRequestException("errors.user.blocked", identityUser.BlockedUntil!.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            else
            {
                identityUser.BlockedUntil = null;
                await userStore.UpdateAsync(identityUser, true);
            }
        }
    }
}
