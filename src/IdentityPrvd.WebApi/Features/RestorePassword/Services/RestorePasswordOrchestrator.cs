using IdentityPrvd.WebApi.Db.Entities;
using IdentityPrvd.WebApi.Exceptions;
using IdentityPrvd.WebApi.Features.RestorePassword.DataAccess;
using IdentityPrvd.WebApi.Features.RestorePassword.Dtos;
using IdentityPrvd.WebApi.Protections;

namespace IdentityPrvd.WebApi.Features.RestorePassword.Services;

public class RestorePasswordOrchestrator(
    TimeProvider timeProvider,
    ConfirmRepo confirmRepo,
    PasswordRepo passwordRepo,
    UserRepo userRepo,
    IHasher hasher)
{
    public async Task RestorePasswordAsync(RestorePasswordDto dto)
    {
        await using var transaction = await confirmRepo.BeginTransactionAsync();

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        var restoreToConfirm = await GetVerifyAsync(dto, utcNow);
        restoreToConfirm.IsActivated = true;
        restoreToConfirm.ActivatedAt = utcNow;
        await confirmRepo.UpdateAsync(restoreToConfirm);

        var userId = restoreToConfirm.UserId;

        await DeactivatedPasswordsAsync(userId, utcNow);

        var passwordHash = hasher.GetHash(dto.Password);

        await SetupUserPasswordAsync(userId, passwordHash);

        await CreateNewPasswordAsync(userId, passwordHash, dto.Hint, utcNow);

        await transaction.CommitAsync();
    }

    private async Task<IdentityConfirm> GetVerifyAsync(RestorePasswordDto dto, DateTime utcNow)
    {
        var restoreToConfirm = await confirmRepo.GetConfirmByVerifyIdAsync(dto.VerifyId)
            ?? throw new BadRequestException("Restore verify id is not correct");

        if (restoreToConfirm.IsActivated)
            throw new BadRequestException("Verify is already activated");

        if (restoreToConfirm.ActiveFrom < utcNow || restoreToConfirm.ActiveTo > utcNow)
            throw new BadRequestException("Verify out of time");

        if (!hasher.Verify(restoreToConfirm.Code, dto.Code))
            throw new BadRequestException("Code is not valid");

        return restoreToConfirm;
    }

    private async Task DeactivatedPasswordsAsync(Ulid userId, DateTime utcNow)
    {
        var userPasswords = await passwordRepo.GetUserPasswordsAsync(userId);
        if (userPasswords.Count != 0)
        {
            var activePasswords = userPasswords.Where(s => s.IsActive);
            foreach (var userPassword in activePasswords)
            {
                userPassword.IsActive = false;
                userPassword.DeactivatedAt = utcNow;
            }

            await passwordRepo.UpdateRangeAsync(activePasswords);
        }
    }

    private async Task SetupUserPasswordAsync(Ulid userId, string passwordHash)
    {
        var userForUpdate = await userRepo.GetUserAysnc(userId);
        userForUpdate.PasswordHash = passwordHash;
        await userRepo.UpdateAsync(userForUpdate);
    }

    private async Task CreateNewPasswordAsync(Ulid userId, string passwordHash, string hint, DateTime utcNow)
    {
        var newPassword = new IdentityPassword
        {
            PasswordHash = passwordHash,
            IsActive = true,
            ActivatedAt = utcNow,
            Hint = hint,
            UserId = userId,
        };

        await passwordRepo.AddAsync(newPassword);
    }
}
