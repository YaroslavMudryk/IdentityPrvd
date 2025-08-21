using IdentityPrvd.Infrastructure.Database.Context.Configs;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Infrastructure.Database.Extensions;

public static class DbContextOptionsBuilderExtensions
{
    public static void AutoConfigureDbContext(this DbContextOptionsBuilder options, string connectionString)
    {
        var databaseType = ConnectionStringHelper.DetectDatabaseType(connectionString);
        if (databaseType == DatabaseType.Unknown)
            throw new ArgumentException("Unknown database. Please config your own", nameof(connectionString));

        if (databaseType == DatabaseType.Postgres)
            options.UseNpgsql(connectionString);
        else if (databaseType == DatabaseType.MySql)
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        else if (databaseType == DatabaseType.SqlServer)
            options.UseSqlServer(connectionString);
        else if (databaseType == DatabaseType.Sqlite)
            options.UseSqlite(connectionString);
        else if (databaseType == DatabaseType.Oracle)
            options.UseOracle(connectionString);
        options.UseInMemoryDatabase("DefaultDb");
    }
}
