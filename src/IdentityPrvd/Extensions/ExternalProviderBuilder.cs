using IdentityPrvd.Options;
using IdentityPrvd.Services.AuthSchemes;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Extensions;

/// <summary>
/// Builder for external authentication providers
/// </summary>
public class ExternalProviderBuilder
{
    private readonly IServiceCollection _services;

    public ExternalProviderBuilder(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Add Google provider
    /// </summary>
    public ExternalProviderBuilder AddGoogle(string clientId, string clientSecret, string callbackPath = null)
    {
        _services.AddGoogleProvider(clientId, clientSecret, callbackPath);
        return this;
    }

    /// <summary>
    /// Add Microsoft provider
    /// </summary>
    public ExternalProviderBuilder AddMicrosoft(string clientId, string clientSecret, string callbackPath = null)
    {
        _services.AddMicrosoftProvider(clientId, clientSecret, callbackPath);
        return this;
    }

    /// <summary>
    /// Add GitHub provider
    /// </summary>
    public ExternalProviderBuilder AddGitHub(string clientId, string clientSecret, string callbackPath = null, IEnumerable<string> scopes = null)
    {
        _services.AddGitHubProvider(clientId, clientSecret, callbackPath, scopes);
        return this;
    }

    /// <summary>
    /// Add Facebook provider
    /// </summary>
    public ExternalProviderBuilder AddFacebook(string clientId, string clientSecret, string callbackPath = null)
    {
        _services.AddFacebookProvider(clientId, clientSecret, callbackPath);
        return this;
    }

    /// <summary>
    /// Add Twitter provider
    /// </summary>
    public ExternalProviderBuilder AddTwitter(string clientId, string clientSecret, string callbackPath = null, IEnumerable<string> scopes = null)
    {
        _services.AddTwitterProvider(clientId, clientSecret, callbackPath, scopes);
        return this;
    }

    /// <summary>
    /// Add Steam provider
    /// </summary>
    public ExternalProviderBuilder AddSteam(string applicationKey, string callbackPath = null)
    {
        _services.AddSteamProvider(applicationKey, callbackPath);
        return this;
    }

    /// <summary>
    /// Add custom provider
    /// </summary>
    public ExternalProviderBuilder AddCustom<TConfigurator>(ExternalProviderOptions options) where TConfigurator : class, IExternalProviderConfigurator
    {
        _services.AddExternalProvider<TConfigurator>(options);
        return this;
    }

    /// <summary>
    /// Return to main builder
    /// </summary>
    public IdentityPrvdBuilder And()
    {
        return new IdentityPrvdBuilder(_services, new IdentityPrvdOptions());
    }
}
