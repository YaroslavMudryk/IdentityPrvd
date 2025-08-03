using IdentityPrvd.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Services.AuthSchemes;

public static class ExternalProviderBuilderExtensions
{
    /// <summary>
    /// Adds an external authentication provider using a generic configurator
    /// </summary>
    /// <typeparam name="TConfigurator">The configurator type implementing IExternalProviderConfigurator</typeparam>
    /// <param name="builder">The authentication builder</param>
    /// <param name="options">The external provider options</param>
    /// <returns>The authentication builder for chaining</returns>
    public static IdentityAuthenticationBuilder AddExternalProvider<TConfigurator>(
        this IdentityAuthenticationBuilder builder, 
        ExternalProviderOptions options) 
        where TConfigurator : class, IExternalProviderConfigurator
    {
        var configurator = Activator.CreateInstance<TConfigurator>();
        configurator.Configure(builder, options);
        return builder;
    }

    /// <summary>
    /// Adds an external authentication provider using a generic configurator with service provider
    /// </summary>
    /// <typeparam name="TConfigurator">The configurator type implementing IExternalProviderConfigurator</typeparam>
    /// <param name="builder">The authentication builder</param>
    /// <param name="options">The external provider options</param>
    /// <param name="serviceProvider">The service provider to resolve dependencies</param>
    /// <returns>The authentication builder for chaining</returns>
    public static IdentityAuthenticationBuilder AddExternalProvider<TConfigurator>(
        this IdentityAuthenticationBuilder builder, 
        ExternalProviderOptions options,
        IServiceProvider serviceProvider) 
        where TConfigurator : class, IExternalProviderConfigurator
    {
        var configurator = serviceProvider.GetRequiredService<TConfigurator>();
        configurator.Configure(builder, options);
        return builder;
    }

    /// <summary>
    /// Adds an external authentication provider using a generic configurator with factory
    /// </summary>
    /// <typeparam name="TConfigurator">The configurator type implementing IExternalProviderConfigurator</typeparam>
    /// <param name="builder">The authentication builder</param>
    /// <param name="options">The external provider options</param>
    /// <param name="configuratorFactory">Factory function to create the configurator</param>
    /// <returns>The authentication builder for chaining</returns>
    public static IdentityAuthenticationBuilder AddExternalProvider<TConfigurator>(
        this IdentityAuthenticationBuilder builder, 
        ExternalProviderOptions options,
        Func<TConfigurator> configuratorFactory) 
        where TConfigurator : class, IExternalProviderConfigurator
    {
        var configurator = configuratorFactory();
        configurator.Configure(builder, options);
        return builder;
    }

    /// <summary>
    /// Adds multiple external authentication providers using IdentityPrvdOptions
    /// </summary>
    /// <param name="builder">The authentication builder</param>
    /// <param name="identityOptions">The IdentityPrvd options containing external providers</param>
    /// <param name="serviceProvider">The service provider to resolve configurators</param>
    /// <returns>The authentication builder for chaining</returns>
    public static IdentityAuthenticationBuilder AddExternalProviders(
        this IdentityAuthenticationBuilder builder,
        IdentityPrvdOptions identityOptions,
        IServiceProvider serviceProvider)
    {
        var providerManager = new ExternalProviderManager(GetConfigurators(serviceProvider));
        providerManager.ConfigureProviders(builder, identityOptions);
        return builder;
    }

    /// <summary>
    /// Adds multiple external authentication providers using IdentityPrvdOptions with custom configurators
    /// </summary>
    /// <param name="builder">The authentication builder</param>
    /// <param name="identityOptions">The IdentityPrvd options containing external providers</param>
    /// <param name="configurators">Collection of configurators</param>
    /// <returns>The authentication builder for chaining</returns>
    public static IdentityAuthenticationBuilder AddExternalProviders(
        this IdentityAuthenticationBuilder builder,
        IdentityPrvdOptions identityOptions,
        IEnumerable<IExternalProviderConfigurator> configurators)
    {
        var providerManager = new ExternalProviderManager(configurators);
        providerManager.ConfigureProviders(builder, identityOptions);
        return builder;
    }

    /// <summary>
    /// Adds a specific external provider by name using the service provider
    /// </summary>
    /// <param name="builder">The authentication builder</param>
    /// <param name="providerName">The name of the provider</param>
    /// <param name="options">The external provider options</param>
    /// <param name="serviceProvider">The service provider to resolve the configurator</param>
    /// <returns>The authentication builder for chaining</returns>
    public static IdentityAuthenticationBuilder AddExternalProviderByName(
        this IdentityAuthenticationBuilder builder,
        string providerName,
        ExternalProviderOptions options,
        IServiceProvider serviceProvider)
    {
        var configuratorType = GetConfiguratorType(providerName);
        if (configuratorType != null)
        {
            var configurator = (IExternalProviderConfigurator)serviceProvider.GetRequiredService(configuratorType);
            configurator.Configure(builder, options);
        }
        return builder;
    }

    private static IEnumerable<IExternalProviderConfigurator> GetConfigurators(IServiceProvider serviceProvider)
    {
        return new IExternalProviderConfigurator[]
        {
            serviceProvider.GetRequiredService<GoogleProviderConfigurator>(),
            serviceProvider.GetRequiredService<MicrosoftProviderConfigurator>(),
            serviceProvider.GetRequiredService<GitHubProviderConfigurator>(),
            serviceProvider.GetRequiredService<FacebookProviderConfigurator>(),
            serviceProvider.GetRequiredService<TwitterProviderConfigurator>(),
            serviceProvider.GetRequiredService<SteamProviderConfigurator>()
        };
    }

    private static Type? GetConfiguratorType(string providerName)
    {
        return providerName.ToLowerInvariant() switch
        {
            "google" => typeof(GoogleProviderConfigurator),
            "microsoft" => typeof(MicrosoftProviderConfigurator),
            "github" => typeof(GitHubProviderConfigurator),
            "facebook" => typeof(FacebookProviderConfigurator),
            "twitter" => typeof(TwitterProviderConfigurator),
            "steam" => typeof(SteamProviderConfigurator),
            _ => null
        };
    }
} 