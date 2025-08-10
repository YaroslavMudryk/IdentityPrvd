using IdentityPrvd.Extensions.Old;
using IdentityPrvd.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace IdentityPrvd.Services.AuthSchemes;

/// <summary>
/// Unified extension methods for external authentication providers
/// Handles registration, configuration, and setup in a single call
/// </summary>
public static class UnifiedExternalProviderExtensions
{
    /// <summary>
    /// Adds an external authentication provider with automatic registration and configuration
    /// </summary>
    /// <typeparam name="TConfigurator">The configurator type implementing IExternalProviderConfigurator</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="options">The external provider options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddExternalProvider<TConfigurator>(
        this IServiceCollection services,
        ExternalProviderOptions options)
        where TConfigurator : class, IExternalProviderConfigurator
    {
        // Register the configurator
        services.AddScoped<TConfigurator>();
        
        // Register the provider manager if not already registered
        services.TryAddScoped<ExternalProviderManager>();
        
        // Store the configuration for later use during authentication setup
        services.Configure<ExternalProviderConfiguration>(config =>
        {
            config.Providers[typeof(TConfigurator).Name] = options;
        });
        
        return services;
    }

    /// <summary>
    /// Adds Google authentication provider
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="clientId">Google client ID</param>
    /// <param name="clientSecret">Google client secret</param>
    /// <param name="callbackPath">Optional custom callback path</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddGoogleProvider(
        this IServiceCollection services,
        string clientId,
        string clientSecret,
        string? callbackPath = null)
    {
        var options = new ExternalProviderOptions
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            CallbackPath = callbackPath ?? "/signin-google",
            IsAvailable = true
        };
        
        return services.AddExternalProvider<GoogleProviderConfigurator>(options);
    }

    /// <summary>
    /// Adds Microsoft authentication provider
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="clientId">Microsoft client ID</param>
    /// <param name="clientSecret">Microsoft client secret</param>
    /// <param name="callbackPath">Optional custom callback path</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMicrosoftProvider(
        this IServiceCollection services,
        string clientId,
        string clientSecret,
        string? callbackPath = null)
    {
        var options = new ExternalProviderOptions
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            CallbackPath = callbackPath ?? "/signin-microsoft",
            IsAvailable = true
        };
        
        return services.AddExternalProvider<MicrosoftProviderConfigurator>(options);
    }

    /// <summary>
    /// Adds GitHub authentication provider
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="clientId">GitHub client ID</param>
    /// <param name="clientSecret">GitHub client secret</param>
    /// <param name="callbackPath">Optional custom callback path</param>
    /// <param name="scopes">Optional custom scopes</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddGitHubProvider(
        this IServiceCollection services,
        string clientId,
        string clientSecret,
        string? callbackPath = null,
        IEnumerable<string>? scopes = null)
    {
        var options = new ExternalProviderOptions
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            CallbackPath = callbackPath ?? "/signin-github",
            IsAvailable = true,
            Scopes = scopes?.ToList() ?? ["read:user", "user:email"]
        };
        
        return services.AddExternalProvider<GitHubProviderConfigurator>(options);
    }

    /// <summary>
    /// Adds Facebook authentication provider
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="clientId">Facebook app ID</param>
    /// <param name="clientSecret">Facebook app secret</param>
    /// <param name="callbackPath">Optional custom callback path</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddFacebookProvider(
        this IServiceCollection services,
        string clientId,
        string clientSecret,
        string? callbackPath = null)
    {
        var options = new ExternalProviderOptions
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            CallbackPath = callbackPath ?? "/signin-facebook",
            IsAvailable = true
        };
        
        return services.AddExternalProvider<FacebookProviderConfigurator>(options);
    }

    /// <summary>
    /// Adds Twitter authentication provider
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="clientId">Twitter client ID</param>
    /// <param name="clientSecret">Twitter client secret</param>
    /// <param name="callbackPath">Optional custom callback path</param>
    /// <param name="scopes">Optional custom scopes</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddTwitterProvider(
        this IServiceCollection services,
        string clientId,
        string clientSecret,
        string? callbackPath = null,
        IEnumerable<string>? scopes = null)
    {
        var options = new ExternalProviderOptions
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            CallbackPath = callbackPath ?? "/signin-twitter",
            IsAvailable = true,
            Scopes = scopes?.ToList() ?? ["users.read", "users.email"]
        };
        
        return services.AddExternalProvider<TwitterProviderConfigurator>(options);
    }

    /// <summary>
    /// Adds Steam authentication provider
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="applicationKey">Steam application key</param>
    /// <param name="callbackPath">Optional custom callback path</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddSteamProvider(
        this IServiceCollection services,
        string applicationKey,
        string? callbackPath = null)
    {
        var options = new ExternalProviderOptions
        {
            ClientId = applicationKey,
            CallbackPath = callbackPath ?? "/signin-steam",
            IsAvailable = true
        };
        
        return services.AddExternalProvider<SteamProviderConfigurator>(options);
    }

    /// <summary>
    /// Configures all registered external providers in the authentication builder
    /// </summary>
    /// <param name="builder">The authentication builder</param>
    /// <param name="serviceProvider">The service provider</param>
    /// <returns>The authentication builder for chaining</returns>
    public static IdentityAuthenticationBuilder ConfigureExternalProviders(
        this IdentityAuthenticationBuilder builder,
        IServiceProvider serviceProvider)
    {
        var config = serviceProvider.GetService<IOptions<ExternalProviderConfiguration>>()?.Value;
        if (config == null) return builder;

        var providerManager = serviceProvider.GetRequiredService<ExternalProviderManager>();
        
        // Create IdentityPrvdOptions with the configured providers
        var identityOptions = new IdentityPrvdOptions();
        foreach (var provider in config.Providers)
        {
            identityOptions.ExternalProviders[provider.Key] = provider.Value;
        }
        
        // Configure all providers
        providerManager.ConfigureProviders(builder, identityOptions);
        
        return builder;
    }
}

/// <summary>
/// Configuration holder for external providers
/// </summary>
public class ExternalProviderConfiguration
{
    public Dictionary<string, ExternalProviderOptions> Providers { get; set; } = new();
} 