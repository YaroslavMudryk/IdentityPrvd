using IdentityPrvd.Data.Stores;
using IdentityPrvd.Data.Transactions;
using IdentityPrvd.Domain.Entities;

namespace IdentityPrvd.Services.Security;

public class UserSecureService(
    ITransactionManager transactionManager,
    IUserStore userStore,
    IBanStore banStore,
    IFailedLoginAttemptStore failedLoginAttemptStore) : IUserSecureService
{
    public async Task IncrementFailedLoginByBlockAsync(IdentityUser user, DateTime utcNow)
    {
        await using var transaction = await transactionManager.BeginTransactionAsync();

        user.FailedLoginAttemptsCount = 0;
        user.BlockedUntil = utcNow.AddHours(1);
        await userStore.UpdateAsync(user);
        await banStore.AddAsync(new IdentityBan
        {
            Cause = "Failed login attempts counts to 5",
            UserId = user.Id,
            Start = utcNow,
            End = user.BlockedUntil.Value,
        });
        await failedLoginAttemptStore.AddAsync(new IdentityFailedLoginAttempt
        {
            UserId = user.Id,
            Client = default!,
            Location = default!,
            Login = user.Login,
            Password = string.Empty, // Password is not stored for security reasons
        });

        await transaction.CommitAsync();
    }

    public async Task IncrementFailedLoginByPasswordAsync(IdentityUser user, DateTime utcNow)
    {
        await using var transaction = await transactionManager.BeginTransactionAsync();

        user.FailedLoginAttemptsCount++;
        await userStore.UpdateAsync(user);
        await failedLoginAttemptStore.AddAsync(new IdentityFailedLoginAttempt
        {
            UserId = user.Id,
            Client = default!,
            Location = default!,
            Login = user.Login,
            Password = string.Empty, // Password is not stored for security reasons
        });

        await transaction.CommitAsync();
    }
}
