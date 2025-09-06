using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Data.Transactions;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Features.Authentication.RestorePassword.Dtos;
using IdentityPrvd.Services.Security;

namespace IdentityPrvd.Features.Authentication.RestorePassword.Services;

public class RestorePasswordOrchestrator(
    TimeProvider timeProvider,
    IConfirmStore confirmStore,
    IPasswordStore passwordStore,
    IUserStore userStore,
    ITransactionManager transactionManager,
    IHasher hasher)
{
    public async Task RestorePasswordAsync(RestorePasswordDto dto)
    {
        await using var transaction = await transactionManager.BeginTransactionAsync();

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        var restoreToConfirm = await GetVerifyAsync(dto, utcNow);
        restoreToConfirm.IsActivated = true;
        restoreToConfirm.ActivatedAt = utcNow;
        await confirmStore.UpdateAsync(restoreToConfirm);

        var userId = restoreToConfirm.UserId;

        await DeactivatedPasswordsAsync(userId, utcNow);

        var passwordHash = hasher.GetHash(dto.Password);

        await SetupUserPasswordAsync(userId, passwordHash);

        await CreateNewPasswordAsync(userId, passwordHash, dto.Hint, utcNow);

        await transaction.CommitAsync();
    }

    private async Task<IdentityConfirm> GetVerifyAsync(RestorePasswordDto dto, DateTime utcNow)
    {
        var restoreToConfirm = await confirmStore.GetConfirmByCodeAsync(dto.VerifyId)
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
        var userPasswords = await passwordStore.GetUserPasswordsAsync(userId);
        if (userPasswords.Count != 0)
        {
            var activePasswords = userPasswords.Where(s => s.IsActive);
            foreach (var userPassword in activePasswords)
            {
                userPassword.IsActive = false;
                userPassword.DeactivatedAt = utcNow;
            }

            await passwordStore.UpdateRangeAsync(activePasswords);
        }
    }

    private async Task SetupUserPasswordAsync(Ulid userId, string passwordHash)
    {
        var userForUpdate = await userStore.GetUserAysnc(userId);
        userForUpdate.PasswordHash = passwordHash;
        await userStore.UpdateAsync(userForUpdate);
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

        await passwordStore.AddAsync(newPassword);
    }
}
