using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Infrastructure.Database.Context;

public class SqliteProviderStrategy : IDatabaseProviderStrategy
{
    public string ProviderName => "Sqlite";

    public void ConfigureDbContext(DbContextOptionsBuilder optionsBuilder, string connectionString)
    {
        optionsBuilder.UseSqlite(connectionString).UseSnakeCaseNamingConvention();
    }

    public void ConfigureModel(ModelBuilder modelBuilder)
    {

    }
}
