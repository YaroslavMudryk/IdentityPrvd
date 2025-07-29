using FluentValidation;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Common.Helpers;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Data.Transactions;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Domain.Enums;
using IdentityPrvd.Features.Authentication.Signup.Dtos;
using IdentityPrvd.Options;
using IdentityPrvd.Services.Notification;
using IdentityPrvd.Services.Security;

namespace IdentityPrvd.Features.Authentication.Signup.Services;

public class SignupOrchestrator(
    IUserStore userStore,
    IUserRoleStore userRoleStore,
    IConfirmStore confirmStore,
    IPasswordStore passwordStore,
    IRolesQuery rolesQuery,
    IUserContext userContext,
    IHasher hasher,
    IEmailService emailService,
    ISmsService smsService,
    IValidator<SignupRequestDto> validator,
    ITransactionManager transactionManager,
    TimeProvider timeProvider,
    IdentityPrvdOptions options)
{
    public async Task<SignupResponseDto> SignupAsync(SignupRequestDto dto)
    {
        await validator.ValidateAndThrowAsync(dto);

        await using var transaction = await transactionManager.BeginTransactionAsync();

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        var user = new IdentityUser
        {
            Id = Ulid.NewUlid(),
            Login = dto.Login,
            PasswordHash = hasher.GetHash(dto.Password),
            UserName = dto.UserName ?? Guid.NewGuid().ToString("N")[..10],
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            CanBeBlocked = true,
            IsConfirmed = !options.UserOptions.ConfirmRequired,
            ConfirmedAt = !options.UserOptions.ConfirmRequired ? utcNow : null,
            ConfirmedBy = !options.UserOptions.ConfirmRequired ? userContext.GetBy<ServiceUser>() : null
        };

        await userStore.AddAsync(user);

        var userRole = new IdentityUserRole
        {
            UserId = user.Id,
            RoleId = await rolesQuery.GetDefaultRoleRoleIdAsync()
        };
        await userRoleStore.AddAsync(userRole);

        var password = new IdentityPassword
        {
            IsActive = true,
            ActivatedAt = utcNow,
            PasswordHash = user.PasswordHash,
            UserId = user.Id,
        };
        await passwordStore.AddAsync(password);

        if (options.UserOptions.ConfirmRequired)
        {
            var codeToConfirm = GetCodeToConfirm(user.Login);
            var confirmCode = new IdentityConfirm
            {
                ActiveFrom = utcNow,
                ActiveTo = utcNow.AddMinutes(options.UserOptions.ConfirmCodeValidInMinutes),
                VerifyId = Guid.NewGuid().ToString("N"),
                Code = Generator.GetString(6),
                UserId = user.Id,
                Type = ConfirmType.User,
            };
            await confirmStore.AddAsync(confirmCode);
            await SendVerificationAsync(codeToConfirm, user.Login);
        }

        await transaction.CommitAsync();

        return new SignupResponseDto
        {
            UserId = user.Id.ToString(),
            Login = user.Login,
            UserName = user.UserName
        };
    }

    private static string GetCodeToConfirm(string login)
    {
        if (LoginExtensions.IsEmail(login))
        {
            return Generator.GetString(50);
        }
        else
        {
            return Random.Shared.Next(10000000, 99999999).ToString();
        }
    }

    private async Task SendVerificationAsync(string code, string login)
    {
        if (LoginExtensions.IsPhone(login))
            await smsService.SendSmsAsync(login, code);
        else
            await emailService.SendEmailAsync(login, "Confirmation account", code);
    }
}
