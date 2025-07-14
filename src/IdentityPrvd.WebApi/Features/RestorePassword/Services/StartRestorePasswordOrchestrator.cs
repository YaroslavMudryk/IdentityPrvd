using IdentityPrvd.WebApi.Db;
using IdentityPrvd.WebApi.Db.Entities;
using IdentityPrvd.WebApi.EmailSender;
using IdentityPrvd.WebApi.Exceptions;
using IdentityPrvd.WebApi.Extensions;
using IdentityPrvd.WebApi.Features.RestorePassword.DataAccess;
using IdentityPrvd.WebApi.Features.RestorePassword.Dtos;
using IdentityPrvd.WebApi.Helpers;
using IdentityPrvd.WebApi.Protections;
using IdentityPrvd.WebApi.SmsSender;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.WebApi.Features.RestorePassword.Services;

public class StartRestorePasswordOrchestrator(
    IdentityPrvdContext dbContext,
    TimeProvider timeProvider,
    ConfirmRepo confirmRepo,
    ISmsService smsService,
    IEmailService emailService,
    IHasher hasher)
{
    public async Task<StartedRestorePasswordDto> StartRestorePasswordAsync(StartRestorePasswordDto dto)
    {
        var userToRestorePassword = await dbContext.Users.FirstOrDefaultAsync(s => s.Login == dto.Login)
            ?? throw new BadRequestException("This user can't be restore password");

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var code = GetCode(userToRestorePassword.Login);
        var confirm = new IdentityConfirm
        {
            Type = Db.Entities.Enums.ConfirmType.RestorePassword,
            UserId = userToRestorePassword.Id,
            ActiveFrom = utcNow,
            ActiveTo = utcNow.AddMinutes(15),
            Code = hasher.GetHash(code),
            VerifyId = Generator.GetString(50),
            IsActivated = false
        };
        await confirmRepo.AddAsync(confirm);

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
