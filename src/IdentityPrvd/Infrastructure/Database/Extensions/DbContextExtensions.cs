using IdentityPrvd.Infrastructure.Database.Audits;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Infrastructure.Database.Extensions;

public static class DbContextExtensions
{
    public static void HardRemove<T>(this DbSet<T> table, T model) where T : class
    {
        if (model is ISoftDeletable baseModel)
        {
            baseModel.HardDelete = true;
            table.Remove(model);
        }
    }

    public static void HardRemove<T>(this DbSet<T> table, IEnumerable<T> models) where T : class
    {
        foreach (var model in models)
        {
            table.HardRemove(model);
        }
    }
}
