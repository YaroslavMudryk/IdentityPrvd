
namespace IdentityPrvd.Data.Transactions;

public interface ITransactionManager
{
    Task<ITransactionScope> BeginTransactionAsync();
}
