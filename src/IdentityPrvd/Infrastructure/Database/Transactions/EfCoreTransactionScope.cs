using IdentityPrvd.Data.Transactions;
using IdentityPrvd.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace IdentityPrvd.Infrastructure.Database.Transactions;

public class EfCoreTransactionScope(IDbContextTransaction transaction, IdentityPrvdContext dbContext) : ITransactionScope
{
    public async Task CommitAsync()
    {
        await transaction.CommitAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await transaction.DisposeAsync();
    }

    public IDbConnection GetUnderlyingConnection()
    {
        return dbContext.Database.GetDbConnection();
    }

    public object GetUnderlyingTransaction()
    {
        return transaction;
    }

    public async Task RollbackAsync()
    {
        await transaction.RollbackAsync();
    }
}
