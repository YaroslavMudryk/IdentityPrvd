using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Common.Helpers;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Domain.Enums;
using IdentityPrvd.Features.Authentication.RestorePassword.Dtos;
using IdentityPrvd.Services.Notification;
using IdentityPrvd.Services.Security;

namespace IdentityPrvd.Features.Authentication.RestorePassword.Services;

public class StartRestorePasswordOrchestrator(
    TimeProvider timeProvider,
    IUsersQuery usersQuery,
    IConfirmStore confirmStore,
    ISmsService smsService,
    IEmailService emailService,
    IHasher hasher)
{
    public async Task<StartedRestorePasswordDto> StartRestorePasswordAsync(StartRestorePasswordDto dto)
    {
        var userToRestorePassword = await usersQuery.GetUserByLoginNullableAsync(dto.Login)
            ?? throw new BadRequestException("This user can't be restore password");

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var code = GetCode(userToRestorePassword.Login);
        var confirm = new IdentityConfirm
        {
            Type = ConfirmType.RestorePassword,
            UserId = userToRestorePassword.Id,
            ActiveFrom = utcNow,
            ActiveTo = utcNow.AddMinutes(15),
            Code = hasher.GetHash(code),
            VerifyId = Generator.GetString(50),
            IsActivated = false
        };
        await confirmStore.AddAsync(confirm);

        await SendRestorePasswordAsync(userToRestorePassword.Login, confirm);

        return new StartedRestorePasswordDto
        {
            Login = dto.Login,
            VerifyId = confirm.VerifyId,
            Code = null
        };
    }

    private async Task SendRestorePasswordAsync(string login, IdentityConfirm confirm)
    {
        if (LoginExtensions.IsEmail(login))
            await emailService.SendEmailAsync(login, "Restore password", confirm.Code);
        else
            await smsService.SendSmsAsync(login, confirm.Code);
    }

    private static string GetCode(string login)
    {
        if (LoginExtensions.IsEmail(login))
            return Generator.GetString(50);
        
        if (LoginExtensions.IsPhone(login))
            return Generator.GetCode(8);

        throw new BadRequestException($"Login is any string, so can't send restore");
    }
}
