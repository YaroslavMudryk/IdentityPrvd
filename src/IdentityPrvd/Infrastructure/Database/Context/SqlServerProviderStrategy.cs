using IdentityPrvd.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityPrvd.Infrastructure.Database.Context;

/// <summary>
/// SQL Server-specific database provider strategy
/// </summary>
public class SqlServerProviderStrategy : IDatabaseProviderStrategy
{
    public string ProviderName => "SQLServer";

    public void ConfigureDbContext(DbContextOptionsBuilder optionsBuilder, string connectionString)
    {
        optionsBuilder.UseSqlServer(connectionString, options =>
        {
            options.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });
    }

    public void ConfigureModel(ModelBuilder modelBuilder)
    {
        // SQL Server-specific configurations
        ConfigureSqlServerSpecificSettings(modelBuilder);
    }

    private void ConfigureSqlServerSpecificSettings(ModelBuilder modelBuilder)
    {
        // Configure all entities for SQL Server
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Set schema for all entities (optional, can be customized)
            if (string.IsNullOrEmpty(entityType.GetSchema()))
            {
                entityType.SetSchema("dbo");
            }

            // Configure SQL Server-specific optimizations
            ConfigureSqlServerOptimizations(modelBuilder, entityType);
        }

        // SQL Server-specific entity configurations
        ConfigureSqlServerEntities(modelBuilder);
    }

    private void ConfigureSqlServerOptimizations(ModelBuilder modelBuilder, Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType)
    {
        // Configure compression for large tables
        if (entityType.ClrType == typeof(Audit))
        {
            modelBuilder.Entity<Audit>(entity =>
            {
                entity.ToTable(tb => tb.IsTemporal());
            });
        }
    }

    private void ConfigureSqlServerEntities(ModelBuilder modelBuilder)
    {
        // Configure SQL Server-specific data types
    }
} 