using IdentityPrvd.Data.Transactions;
using IdentityPrvd.Infrastructure.Database.Context;

namespace IdentityPrvd.Infrastructure.Database.Transactions;

public class EfCoreTransactionManager(
    IdentityPrvdContext dbContext) : ITransactionManager
{
    public async Task<ITransactionScope> BeginTransactionAsync()
    {
        var dbContextTransaction = await dbContext.Database.BeginTransactionAsync();
        return new EfCoreTransactionScope(dbContextTransaction, dbContext);
    }
}
