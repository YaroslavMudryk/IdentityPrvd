using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Infrastructure.Database.Context;

/// <summary>
/// Manages database provider strategies and configurations
/// </summary>
public class DatabaseProviderManager
{
    private readonly Dictionary<string, IDatabaseProviderStrategy> _strategies;
    private readonly IServiceProvider _serviceProvider;

    public DatabaseProviderManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _strategies = new Dictionary<string, IDatabaseProviderStrategy>(StringComparer.OrdinalIgnoreCase);
        
        // Register default strategies
        RegisterDefaultStrategies();
    }

    private void RegisterDefaultStrategies()
    {
        _strategies["PostgreSQL"] = new PostgreSqlProviderStrategy();
        _strategies["SQLServer"] = new SqlServerProviderStrategy();
        _strategies["MySQL"] = new MySqlProviderStrategy();
        _strategies["Sqlite"] = new SqliteProviderStrategy();
    }

    /// <summary>
    /// Registers a custom database provider strategy
    /// </summary>
    public void RegisterStrategy(IDatabaseProviderStrategy strategy)
    {
        _strategies[strategy.ProviderName] = strategy;
    }

    /// <summary>
    /// Gets the database provider strategy for the specified provider
    /// </summary>
    public IDatabaseProviderStrategy GetStrategy(string providerName)
    {
        if (!_strategies.TryGetValue(providerName, out var strategy))
        {
            throw new ArgumentException($"Unsupported database provider: {providerName}");
        }

        return strategy;
    }

    /// <summary>
    /// Configures the DbContext for the specified provider
    /// </summary>
    public void ConfigureDbContext(DbContextOptionsBuilder optionsBuilder, string providerName, string connectionString)
    {
        var strategy = GetStrategy(providerName);
        strategy.ConfigureDbContext(optionsBuilder, connectionString);
    }

    /// <summary>
    /// Applies provider-specific model configurations
    /// </summary>
    public void ConfigureModel(ModelBuilder modelBuilder, string providerName)
    {
        var strategy = GetStrategy(providerName);
        strategy.ConfigureModel(modelBuilder);
    }

    /// <summary>
    /// Gets all available provider names
    /// </summary>
    public IEnumerable<string> GetAvailableProviders()
    {
        return _strategies.Keys;
    }
}

/// <summary>
/// Extension methods for registering database provider manager
/// </summary>
public static class DatabaseProviderManagerExtensions
{
    /// <summary>
    /// Adds the database provider manager to the service collection
    /// </summary>
    public static IServiceCollection AddDatabaseProviderManager(this IServiceCollection services)
    {
        services.AddSingleton<DatabaseProviderManager>();
        return services;
    }

    /// <summary>
    /// Registers a custom database provider strategy
    /// </summary>
    public static IServiceCollection AddDatabaseProviderStrategy<TStrategy>(this IServiceCollection services)
        where TStrategy : class, IDatabaseProviderStrategy
    {
        services.AddScoped<TStrategy>();
        services.AddScoped<IDatabaseProviderStrategy, TStrategy>();
        return services;
    }
} 