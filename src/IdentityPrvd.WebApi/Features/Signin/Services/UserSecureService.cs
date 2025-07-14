using IdentityPrvd.WebApi.Db;
using IdentityPrvd.WebApi.Db.Entities;

namespace IdentityPrvd.WebApi.Features.Signin.Services;

public interface IUserSecureService
{
    Task IncrementFailedLoginByBlockAsync(IdentityUser user, DateTime utcNow);
    Task IncrementFailedLoginByPasswordAsync(IdentityUser user, DateTime utcNow);
}

public class UserSecureService(
    IdentityPrvdContext dbContext) : IUserSecureService
{
    public async Task IncrementFailedLoginByBlockAsync(IdentityUser user, DateTime utcNow)
    {
        user.FailedLoginAttemptsCount = 0;
        user.BlockedUntil = utcNow.AddHours(1);
        dbContext.Users.Update(user);
        await dbContext.Bans.AddAsync(new IdentityBan
        {
            Cause = "Failed login attempts counts to 5",
            UserId = user.Id,
            Start = utcNow,
            End = user.BlockedUntil.Value,
        });
        await dbContext.FailedLoginAttempts.AddAsync(new IdentityFailedLoginAttempt
        {
            UserId = user.Id,
            Client = default!,
            Location = default!,
            Login = user.Login,
            Password = string.Empty, // Password is not stored for security reasons
        });
        await dbContext.SaveChangesAsync();
    }

    public async Task IncrementFailedLoginByPasswordAsync(IdentityUser user, DateTime utcNow)
    {
        user.FailedLoginAttemptsCount++;
        dbContext.Users.Update(user);
        await dbContext.FailedLoginAttempts.AddAsync(new IdentityFailedLoginAttempt
        {
            UserId = user.Id,
            Client = default!,
            Location = default!,
            Login = user.Login,
            Password = string.Empty, // Password is not stored for security reasons
        });
        await dbContext.SaveChangesAsync();
    }
}
