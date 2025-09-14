using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Database.Context;

namespace IdentityPrvd.Data.Stores;

public interface IFailedLoginAttemptStore
{
    Task<IdentityFailedLoginAttempt> AddAsync(IdentityFailedLoginAttempt failedLoginAttempt);
}

public class EfFailedLoginAttemptStore(IdentityPrvdContext dbContext) : IFailedLoginAttemptStore
{
    public async Task<IdentityFailedLoginAttempt> AddAsync(IdentityFailedLoginAttempt failedLoginAttempt)
    {
        await dbContext.AddAsync(failedLoginAttempt);
        await dbContext.SaveChangesAsync();
        return failedLoginAttempt;
    }
}
