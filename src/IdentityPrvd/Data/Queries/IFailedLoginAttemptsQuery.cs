using IdentityPrvd.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Queries;

public interface IFailedLoginAttemptsQuery
{
    Task<int> CountFailedAttemptsByLoginAsync(string login, DateTime since);    
    Task<int> CountFailedAttemptsByUserIdAsync(Guid userId, DateTime since);
}

public class EfFailedLoginAttemptsQuery(IdentityPrvdContext dbContext) : IFailedLoginAttemptsQuery
{
    public async Task<int> CountFailedAttemptsByLoginAsync(string login, DateTime since)
    {
        return await dbContext.FailedLoginAttempts
            .AsNoTracking()
            .Where(a => a.Login == login && a.CreatedAt >= since)
            .CountAsync();
    }

    public async Task<int> CountFailedAttemptsByUserIdAsync(Guid userId, DateTime since)
    {
        return await dbContext.FailedLoginAttempts
            .AsNoTracking()
            .Where(a => a.UserId == userId && a.CreatedAt >= since)
            .CountAsync();
    }
}
