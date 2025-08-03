using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Infrastructure.Database.Context;

/// <summary>
/// PostgreSQL-specific database provider strategy
/// </summary>
public class PostgreSqlProviderStrategy : IDatabaseProviderStrategy
{
    public string ProviderName => "PostgreSQL";

    public void ConfigureDbContext(DbContextOptionsBuilder optionsBuilder, string connectionString)
    {
        optionsBuilder.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
    }

    public void ConfigureModel(ModelBuilder modelBuilder)
    {
        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityPrvdContext).Assembly);
        ConfigurePostgreSqlSpecificSettings(modelBuilder);
    }

    private void ConfigurePostgreSqlSpecificSettings(ModelBuilder modelBuilder)
    {
        // PostgreSQL-specific optimizations
        ConfigurePostgreSqlOptimizations(modelBuilder);
    }

    private void ConfigurePostgreSqlOptimizations(ModelBuilder modelBuilder)
    {

    }
}
