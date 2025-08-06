using IdentityPrvd.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Infrastructure.Database.Context;

/// <summary>
/// MySQL-specific database provider strategy
/// </summary>
public class MySqlProviderStrategy : IDatabaseProviderStrategy
{
    public string ProviderName => "MySQL";

    public void ConfigureDbContext(DbContextOptionsBuilder optionsBuilder, string connectionString)
    {
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), options =>
        {
            options.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });
    }

    public void ConfigureModel(ModelBuilder modelBuilder)
    {
        // MySQL-specific configurations
        ConfigureMySqlSpecificSettings(modelBuilder);
    }

    private void ConfigureMySqlSpecificSettings(ModelBuilder modelBuilder)
    {
        // Configure all entities for MySQL
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Configure MySQL-specific optimizations
            ConfigureMySqlOptimizations(modelBuilder, entityType);
        }

        // MySQL-specific entity configurations
        ConfigureMySqlEntities(modelBuilder);
    }

    private void ConfigureMySqlOptimizations(ModelBuilder modelBuilder, Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType)
    {
        // Configure partitioning for large tables (example)
        if (entityType.ClrType == typeof(Audit))
        {
            modelBuilder.Entity<Audit>(entity =>
            {
                entity.ToTable(tb => tb.HasComment("Audit trail table"));
            });
        }
    }

    private void ConfigureMySqlEntities(ModelBuilder modelBuilder)
    {
        // Configure MySQL-specific data types
    }
} 