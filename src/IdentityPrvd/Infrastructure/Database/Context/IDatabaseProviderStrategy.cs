using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Infrastructure.Database.Context;

/// <summary>
/// Strategy interface for database provider-specific configurations
/// </summary>
public interface IDatabaseProviderStrategy
{
    /// <summary>
    /// Gets the provider name
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Applies provider-specific configurations to the model builder
    /// </summary>
    void ConfigureModel(ModelBuilder modelBuilder);

    /// <summary>
    /// Configures the DbContext options for the specific provider
    /// </summary>
    void ConfigureDbContext(DbContextOptionsBuilder optionsBuilder, string connectionString);
} 