using IdentityPrvd.DependencyInjection.Auth;
using IdentityPrvd.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.DependencyInjection.Auth;

public interface IExternalProvidersBuilder
{
    AuthenticationBuilder Authentication { get; }
    IdentityPrvdOptions Options { get; }
    IExternalProvidersBuilder AddGoogle();
    IExternalProvidersBuilder AddMicrosoft();
    IExternalProvidersBuilder AddGitHub();
    IExternalProvidersBuilder AddFacebook();
    IExternalProvidersBuilder AddTwitter();
    IExternalProvidersBuilder AddSteam();
    IExternalProvidersBuilder AddCustomProvider<TProvider>() where TProvider : class, ICustomExternalProvider, new();
}

internal class ExternalProvidersBuilder(IIdentityPrvdBuilder builder) : IExternalProvidersBuilder
{
    private readonly IIdentityPrvdBuilder _builder = builder;
    public AuthenticationBuilder Authentication => _builder.AuthenticationBuilder;
    public IdentityPrvdOptions Options => _builder.Options;

    public IExternalProvidersBuilder AddGoogle()
    {
        var defaults = TryGetDefaults("Google");
        _builder.AuthenticationBuilder.AddGoogle(options =>
        {
            options.ClientId = defaults.ClientId;
            options.ClientSecret = defaults.ClientSecret;
            options.CallbackPath = "/signin-google";
            options.SignInScheme = "cookie";
            options.SaveTokens = true;
            var effectiveScopes = defaults.Scopes;
            if (effectiveScopes != null && effectiveScopes.Count > 0)
            {
                foreach (var scope in effectiveScopes)
                    options.Scope.Add(scope);
            }
        });
        return this;
    }

    public IExternalProvidersBuilder AddMicrosoft()
    {
        var defaults = TryGetDefaults("Microsoft");
        _builder.AuthenticationBuilder.AddMicrosoftAccount(options =>
        {
            options.ClientId = defaults.ClientId;
            options.ClientSecret = defaults.ClientSecret;
            options.CallbackPath = "/signin-microsoft";
            options.SignInScheme = "cookie";
            options.SaveTokens = true;
        });
        return this;
    }

    public IExternalProvidersBuilder AddGitHub()
    {
        var defaults = TryGetDefaults("GitHub");
        _builder.AuthenticationBuilder.AddGitHub(options =>
        {
            options.ClientId = defaults.ClientId;
            options.ClientSecret = defaults.ClientSecret;
            options.CallbackPath = "/signin-github";
            options.SignInScheme = "cookie";
            var effectiveScopes = (defaults.Scopes?.ToList() ?? (defaults.Scopes?.Count > 0 ? defaults.Scopes : ["read:user", "user:email"]))!;
            foreach (var scope in effectiveScopes)
                options.Scope.Add(scope);
            options.SaveTokens = true;
        });
        return this;
    }

    public IExternalProvidersBuilder AddFacebook()
    {
        var defaults = TryGetDefaults("Facebook");
        _builder.AuthenticationBuilder.AddFacebook(options =>
        {
            options.ClientId = defaults.ClientId;
            options.ClientSecret = defaults.ClientSecret;
            options.CallbackPath = "/signin-facebook";
            options.SignInScheme = "cookie";
            options.SaveTokens = true;
        });
        return this;
    }

    public IExternalProvidersBuilder AddTwitter()
    {
        var defaults = TryGetDefaults("Twitter");
        _builder.AuthenticationBuilder.AddTwitter(options =>
        {
            options.ClientId = defaults.ClientId;
            options.ClientSecret = defaults.ClientSecret;
            options.CallbackPath = "/signin-twitter";
            options.SignInScheme = "cookie";
            var effectiveScopes = (defaults.Scopes?.ToList() ?? (defaults.Scopes?.Count > 0 ? defaults.Scopes : ["users.read", "users.email"]))!;
            foreach (var scope in effectiveScopes)
                options.Scope.Add(scope);
            options.SaveTokens = true;
        });
        return this;
    }

    public IExternalProvidersBuilder AddSteam()
    {
        var defaults = TryGetDefaults("Steam");
        _builder.AuthenticationBuilder.AddSteam(options =>
        {
            options.ApplicationKey = defaults.ClientId;
            options.CallbackPath = "/signin-steam";
            options.SignInScheme = "cookie";
            options.SaveTokens = true;
        });
        return this;
    }

    public IExternalProvidersBuilder AddCustomProvider<TProvider>() where TProvider : class, ICustomExternalProvider, new()
    {
        _builder.Services.AddScoped<ICustomExternalProvider, TProvider>();
        var provider = new TProvider();
        provider.Register(_builder.AuthenticationBuilder, _builder.Options);
        return this;
    }

    private ExternalProviderOptions TryGetDefaults(string providerName)
    {
        var defaults = _builder.Options.ExternalProviders.TryGetValue(providerName, out var options)
            ? options
            : new ExternalProviderOptions();
        return defaults;
    }
}
