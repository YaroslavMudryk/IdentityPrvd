using IdentityPrvd.Extensions.Old;
using IdentityPrvd.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Services.AuthSchemes;

public static class ExternalProviderFluentExtensions
{
    /// <summary>
    /// Fluent builder for configuring external authentication providers
    /// </summary>
    /// <param name="builder">The authentication builder</param>
    /// <returns>The external provider fluent builder</returns>
    public static ExternalProviderFluentBuilder WithExternalProviders(this IdentityAuthenticationBuilder builder)
    {
        return new ExternalProviderFluentBuilder(builder);
    }

    /// <summary>
    /// Fluent builder for configuring external authentication providers with IdentityPrvdOptions
    /// </summary>
    /// <param name="builder">The authentication builder</param>
    /// <param name="identityOptions">The IdentityPrvd options</param>
    /// <returns>The external provider fluent builder</returns>
        public static ExternalProviderFluentBuilder WithExternalProviders(
        this IdentityAuthenticationBuilder builder,
        IdentityPrvdOptions identityOptions)
    {
        return new ExternalProviderFluentBuilder(builder, identityOptions);
    }
}

/// <summary>
/// Fluent builder for configuring external authentication providers
/// </summary>
public class ExternalProviderFluentBuilder
{
    private readonly IdentityAuthenticationBuilder _builder;
    private readonly IdentityPrvdOptions? _identityOptions;
    private readonly List<IExternalProviderConfigurator> _configurators = new();
    private readonly IServiceProvider? _serviceProvider;

    public ExternalProviderFluentBuilder(IdentityAuthenticationBuilder builder)
    {
        _builder = builder;
    }

    public ExternalProviderFluentBuilder(IdentityAuthenticationBuilder builder, IdentityPrvdOptions identityOptions)
    {
        _builder = builder;
        _identityOptions = identityOptions;
    }

    public ExternalProviderFluentBuilder(IdentityAuthenticationBuilder builder, IServiceProvider serviceProvider)
    {
        _builder = builder;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Adds a specific external provider using generic configurator
    /// </summary>
    /// <typeparam name="TConfigurator">The configurator type</typeparam>
    /// <param name="options">The provider options</param>
    /// <returns>The fluent builder for chaining</returns>
    public ExternalProviderFluentBuilder AddProvider<TConfigurator>(ExternalProviderOptions options)
        where TConfigurator : class, IExternalProviderConfigurator
    {
        if (_serviceProvider != null)
        {
            var configurator = _serviceProvider.GetRequiredService<TConfigurator>();
            configurator.Configure(_builder, options);
        }
        else
        {
            var configurator = Activator.CreateInstance<TConfigurator>();
            configurator.Configure(_builder, options);
        }
        return this;
    }

    /// <summary>
    /// Adds a specific external provider using factory
    /// </summary>
    /// <typeparam name="TConfigurator">The configurator type</typeparam>
    /// <param name="options">The provider options</param>
    /// <param name="configuratorFactory">Factory to create the configurator</param>
    /// <returns>The fluent builder for chaining</returns>
    public ExternalProviderFluentBuilder AddProvider<TConfigurator>(
        ExternalProviderOptions options, 
        Func<TConfigurator> configuratorFactory)
        where TConfigurator : class, IExternalProviderConfigurator
    {
        var configurator = configuratorFactory();
        configurator.Configure(_builder, options);
        return this;
    }

    /// <summary>
    /// Adds Google provider
    /// </summary>
    /// <param name="clientId">Google client ID</param>
    /// <param name="clientSecret">Google client secret</param>
    /// <param name="callbackPath">Optional custom callback path</param>
    /// <returns>The fluent builder for chaining</returns>
    public ExternalProviderFluentBuilder AddGoogle(string clientId, string clientSecret, string? callbackPath = null)
    {
        var options = new ExternalProviderOptions
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            CallbackPath = callbackPath ?? "/signin-google",
            IsAvailable = true
        };
        return AddProvider<GoogleProviderConfigurator>(options);
    }

    /// <summary>
    /// Adds Microsoft provider
    /// </summary>
    /// <param name="clientId">Microsoft client ID</param>
    /// <param name="clientSecret">Microsoft client secret</param>
    /// <param name="callbackPath">Optional custom callback path</param>
    /// <returns>The fluent builder for chaining</returns>
    public ExternalProviderFluentBuilder AddMicrosoft(string clientId, string clientSecret, string? callbackPath = null)
    {
        var options = new ExternalProviderOptions
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            CallbackPath = callbackPath ?? "/signin-microsoft",
            IsAvailable = true
        };
        return AddProvider<MicrosoftProviderConfigurator>(options);
    }

    /// <summary>
    /// Adds GitHub provider
    /// </summary>
    /// <param name="clientId">GitHub client ID</param>
    /// <param name="clientSecret">GitHub client secret</param>
    /// <param name="callbackPath">Optional custom callback path</param>
    /// <param name="scopes">Optional custom scopes</param>
    /// <returns>The fluent builder for chaining</returns>
    public ExternalProviderFluentBuilder AddGitHub(
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
        return AddProvider<GitHubProviderConfigurator>(options);
    }

    /// <summary>
    /// Adds Facebook provider
    /// </summary>
    /// <param name="clientId">Facebook app ID</param>
    /// <param name="clientSecret">Facebook app secret</param>
    /// <param name="callbackPath">Optional custom callback path</param>
    /// <returns>The fluent builder for chaining</returns>
    public ExternalProviderFluentBuilder AddFacebook(string clientId, string clientSecret, string? callbackPath = null)
    {
        var options = new ExternalProviderOptions
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            CallbackPath = callbackPath ?? "/signin-facebook",
            IsAvailable = true
        };
        return AddProvider<FacebookProviderConfigurator>(options);
    }

    /// <summary>
    /// Adds Twitter provider
    /// </summary>
    /// <param name="clientId">Twitter client ID</param>
    /// <param name="clientSecret">Twitter client secret</param>
    /// <param name="callbackPath">Optional custom callback path</param>
    /// <param name="scopes">Optional custom scopes</param>
    /// <returns>The fluent builder for chaining</returns>
    public ExternalProviderFluentBuilder AddTwitter(
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
        return AddProvider<TwitterProviderConfigurator>(options);
    }

    /// <summary>
    /// Adds Steam provider
    /// </summary>
    /// <param name="applicationKey">Steam application key</param>
    /// <param name="callbackPath">Optional custom callback path</param>
    /// <returns>The fluent builder for chaining</returns>
    public ExternalProviderFluentBuilder AddSteam(string applicationKey, string? callbackPath = null)
    {
        var options = new ExternalProviderOptions
        {
            ClientId = applicationKey,
            CallbackPath = callbackPath ?? "/signin-steam",
            IsAvailable = true
        };
        return AddProvider<SteamProviderConfigurator>(options);
    }

    /// <summary>
    /// Configures all available providers from IdentityPrvdOptions
    /// </summary>
    /// <returns>The fluent builder for chaining</returns>
    public ExternalProviderFluentBuilder ConfigureAll()
    {
        if (_identityOptions == null)
            throw new InvalidOperationException("IdentityPrvdOptions not provided. Use WithExternalProviders(builder, identityOptions) instead.");

        if (_serviceProvider == null)
            throw new InvalidOperationException("ServiceProvider not available. Use WithExternalProviders(builder, identityOptions) instead.");

        var providerManager = new ExternalProviderManager(GetConfigurators(_serviceProvider));
        providerManager.ConfigureProviders(_builder, _identityOptions);
        return this;
    }

    /// <summary>
    /// Builds and returns the authentication builder
    /// </summary>
    /// <returns>The authentication builder</returns>
    public IdentityAuthenticationBuilder Build()
    {
        return _builder;
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
} 