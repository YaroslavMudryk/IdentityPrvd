using FluentValidation;
using IdentityPrvd.WebApi.Db.Entities;
using IdentityPrvd.WebApi.Db.Entities.Enums;
using IdentityPrvd.WebApi.EmailSender;
using IdentityPrvd.WebApi.Extensions;
using IdentityPrvd.WebApi.Features.Signup.DataAccess;
using IdentityPrvd.WebApi.Features.Signup.Dtos;
using IdentityPrvd.WebApi.Helpers;
using IdentityPrvd.WebApi.Options;
using IdentityPrvd.WebApi.Protections;
using IdentityPrvd.WebApi.SmsSender;
using IdentityPrvd.WebApi.UserContext;

namespace IdentityPrvd.WebApi.Features.Signup.Services;

public class SignupOrchestrator(
    UserRepo userRepo,
    UserRoleRepo userRoleRepo,
    ConfirmRepo confirmRepo,
    PasswordRepo passwordRepo,
    RolesQuery rolesQuery,
    IUserContext userContext,
    IHasher hasher,
    IEmailService emailService,
    ISmsService smsService,
    IValidator<SignupRequestDto> validator,
    TimeProvider timeProvider,
    IdentityPrvdOptions options)
{
    public async Task<SignupResponseDto> SignupAsync(SignupRequestDto dto)
    {
        await validator.ValidateAndThrowAsync(dto);

        await using var transaction = await userRepo.BeginTransactionAsync();

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        var user = new IdentityUser
        {
            Id = Ulid.NewUlid(),
            Login = dto.Login,
            PasswordHash = hasher.GetHash(dto.Password),
            UserName = dto.UserName ?? Guid.NewGuid().ToString("N")[..10],
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            IsConfirmed = !options.UserOptions.ConfirmRequired,
            ConfirmedAt = !options.UserOptions.ConfirmRequired ? utcNow : null,
            ConfirmedBy = !options.UserOptions.ConfirmRequired ? userContext.GetBy<ServiceUser>() : null
        };

        await userRepo.AddUserAsync(user);

        var userRole = new IdentityUserRole
        {
            UserId = user.Id,
            RoleId = await rolesQuery.GetDefaultRoleRoleIdAsync()
        };
        await userRoleRepo.AddUserRoleAsync(userRole);

        var password = new IdentityPassword
        {
            IsActive = true,
            ActivatedAt = utcNow,
            PasswordHash = user.PasswordHash,
            UserId = user.Id,
        };
        await passwordRepo.AddAsync(password);

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
            await confirmRepo.AddAsync(confirmCode);
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
