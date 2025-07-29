
using System.Data;

namespace IdentityPrvd.Data.Transactions;

public interface ITransactionScope : IAsyncDisposable
{
    Task CommitAsync();
    Task RollbackAsync();
    object GetUnderlyingTransaction();
    IDbConnection GetUnderlyingConnection();
}
