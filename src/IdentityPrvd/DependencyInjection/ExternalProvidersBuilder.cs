using IdentityPrvd.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.DependencyInjection;

public interface IExternalProvidersBuilder
{
	AuthenticationBuilder Authentication { get; }
	IExternalProvidersBuilder AddGoogle(string? clientId = null, string? clientSecret = null, string? callbackPath = null, IEnumerable<string>? scopes = null);
	IExternalProvidersBuilder AddMicrosoft(string? clientId = null, string? clientSecret = null, string? callbackPath = null);
	IExternalProvidersBuilder AddGitHub(string? clientId = null, string? clientSecret = null, string? callbackPath = null, IEnumerable<string>? scopes = null);
	IExternalProvidersBuilder AddFacebook(string? appId = null, string? appSecret = null, string? callbackPath = null);
	IExternalProvidersBuilder AddTwitter(string? clientId = null, string? clientSecret = null, string? callbackPath = null, IEnumerable<string>? scopes = null);
	IExternalProvidersBuilder AddSteam(string? applicationKey = null, string? callbackPath = null);
	IExternalProvidersBuilder AddOAuth(string scheme, Action<OAuthOptions> configure);
	IExternalProvidersBuilder AddOpenIdConnect(string scheme, Action<OpenIdConnectOptions> configure);
	IExternalProvidersBuilder AddScheme<TOptions, THandler>(string scheme, Action<TOptions> configure)
		where TOptions : AuthenticationSchemeOptions, new()
		where THandler : AuthenticationHandler<TOptions>;
	IExternalProvidersBuilder AddOAuth<TOptions, THandler>(string scheme, Action<TOptions> configure)
		where TOptions : OAuthOptions, new()
		where THandler : OAuthHandler<TOptions>;

	// Custom plugin providers
	IExternalProvidersBuilder AddCustomProvider<TProvider>() where TProvider : ICustomExternalProvider, new();
	IExternalProvidersBuilder AddCustomProvider(ICustomExternalProvider provider);

	// Allow adding to options for UI discovery
	IExternalProvidersBuilder RegisterProviderOption(string providerName, ExternalProviderOptions options);
	IExternalProvidersBuilder ConfigureProviderOptions(string providerName, Action<ExternalProviderOptions> configure);
}

internal class ExternalProvidersBuilder(IIdentityPrvdBuilder builder) : IExternalProvidersBuilder
{
	private readonly IIdentityPrvdBuilder _builder = builder;
	public AuthenticationBuilder Authentication => _builder.AuthenticationBuilder;

	public IExternalProvidersBuilder AddGoogle(string? clientId = null, string? clientSecret = null, string? callbackPath = null, IEnumerable<string>? scopes = null)
	{
		var defaults = TryGetDefaults("Google");
		_builder.AuthenticationBuilder.AddGoogle(options =>
		{
			options.ClientId = clientId ?? defaults.ClientId;
			options.ClientSecret = clientSecret ?? defaults.ClientSecret;
			options.CallbackPath = callbackPath ?? (string.IsNullOrWhiteSpace(defaults.CallbackPath) ? "/signin-google" : defaults.CallbackPath);
			options.SignInScheme = "cookie";
			options.SaveTokens = true;
			var effectiveScopes = scopes?.ToList();
			if (effectiveScopes != null && effectiveScopes.Count > 0)
			{
				foreach (var scope in effectiveScopes)
					options.Scope.Add(scope);
			}
		});
		return this;
	}

	public IExternalProvidersBuilder AddMicrosoft(string? clientId = null, string? clientSecret = null, string? callbackPath = null)
	{
		var defaults = TryGetDefaults("Microsoft");
		_builder.AuthenticationBuilder.AddMicrosoftAccount(options =>
		{
			options.ClientId = clientId ?? defaults.ClientId;
			options.ClientSecret = clientSecret ?? defaults.ClientSecret;
			options.CallbackPath = callbackPath ?? (string.IsNullOrWhiteSpace(defaults.CallbackPath) ? "/signin-microsoft" : defaults.CallbackPath);
			options.SignInScheme = "cookie";
			options.SaveTokens = true;
		});
		return this;
	}

	public IExternalProvidersBuilder AddGitHub(string? clientId = null, string? clientSecret = null, string? callbackPath = null, IEnumerable<string>? scopes = null)
	{
		var defaults = TryGetDefaults("GitHub");
		_builder.AuthenticationBuilder.AddGitHub(options =>
		{
			options.ClientId = clientId ?? defaults.ClientId;
			options.ClientSecret = clientSecret ?? defaults.ClientSecret;
			options.CallbackPath = callbackPath ?? (string.IsNullOrWhiteSpace(defaults.CallbackPath) ? "/signin-github" : defaults.CallbackPath);
			options.SignInScheme = "cookie";
			var effectiveScopes = (scopes?.ToList() ?? (defaults.Scopes?.Count > 0 ? defaults.Scopes : ["read:user", "user:email"]))!;
			foreach (var scope in effectiveScopes)
				options.Scope.Add(scope);
			options.SaveTokens = true;
		});
		return this;
	}

	public IExternalProvidersBuilder AddFacebook(string? appId = null, string? appSecret = null, string? callbackPath = null)
	{
		var defaults = TryGetDefaults("Facebook");
		_builder.AuthenticationBuilder.AddFacebook(options =>
		{
			options.ClientId = appId ?? defaults.ClientId;
			options.ClientSecret = appSecret ?? defaults.ClientSecret;
			options.CallbackPath = callbackPath ?? (string.IsNullOrWhiteSpace(defaults.CallbackPath) ? "/signin-facebook" : defaults.CallbackPath);
			options.SignInScheme = "cookie";
			options.SaveTokens = true;
		});
		return this;
	}

	public IExternalProvidersBuilder AddTwitter(string? clientId = null, string? clientSecret = null, string? callbackPath = null, IEnumerable<string>? scopes = null)
	{
		var defaults = TryGetDefaults("Twitter");
		_builder.AuthenticationBuilder.AddTwitter(options =>
		{
			options.ClientId = clientId ?? defaults.ClientId;
			options.ClientSecret = clientSecret ?? defaults.ClientSecret;
			options.CallbackPath = callbackPath ?? (string.IsNullOrWhiteSpace(defaults.CallbackPath) ? "/signin-twitter" : defaults.CallbackPath);
			options.SignInScheme = "cookie";
			var effectiveScopes = (scopes?.ToList() ?? (defaults.Scopes?.Count > 0 ? defaults.Scopes : ["users.read", "users.email"]))!;
			foreach (var scope in effectiveScopes)
				options.Scope.Add(scope);
			options.SaveTokens = true;
		});
		return this;
	}

	public IExternalProvidersBuilder AddSteam(string? applicationKey = null, string? callbackPath = null)
	{
		var defaults = TryGetDefaults("Steam");
		_builder.AuthenticationBuilder.AddSteam(options =>
		{
			options.ApplicationKey = applicationKey ?? defaults.ClientId;
			options.CallbackPath = callbackPath ?? (string.IsNullOrWhiteSpace(defaults.CallbackPath) ? "/signin-steam" : defaults.CallbackPath);
			options.SignInScheme = "cookie";
			options.SaveTokens = true;
		});
		return this;
	}

	public IExternalProvidersBuilder AddOAuth(string scheme, Action<OAuthOptions> configure)
	{
		_builder.AuthenticationBuilder.AddOAuth(scheme, options =>
		{
			configure(options);
		});
		return this;
	}

	public IExternalProvidersBuilder AddOpenIdConnect(string scheme, Action<OpenIdConnectOptions> configure)
	{
		_builder.AuthenticationBuilder.AddOpenIdConnect(scheme, options =>
		{
			configure(options);
		});
		return this;
	}

	public IExternalProvidersBuilder AddScheme<TOptions, THandler>(string scheme, Action<TOptions> configure)
		where TOptions : AuthenticationSchemeOptions, new()
		where THandler : AuthenticationHandler<TOptions>
	{
		_builder.AuthenticationBuilder.AddScheme<TOptions, THandler>(scheme, configure);
		return this;
	}

	public IExternalProvidersBuilder AddOAuth<TOptions, THandler>(string scheme, Action<TOptions> configure)
		where TOptions : OAuthOptions, new()
		where THandler : OAuthHandler<TOptions>
	{
		_builder.AuthenticationBuilder.AddOAuth<TOptions, THandler>(scheme, configure);
		return this;
	}

	public IExternalProvidersBuilder AddCustomProvider<TProvider>() where TProvider : ICustomExternalProvider, new()
	{
		var provider = new TProvider();
		provider.Register(_builder.AuthenticationBuilder, _builder.Options);
		return this;
	}

	public IExternalProvidersBuilder AddCustomProvider(ICustomExternalProvider provider)
	{
		provider.Register(_builder.AuthenticationBuilder, _builder.Options);
		return this;
	}

	public IExternalProvidersBuilder RegisterProviderOption(string providerName, ExternalProviderOptions options)
	{
		_builder.Options.ExternalProviders[providerName] = options;
		return this;
	}

	public IExternalProvidersBuilder ConfigureProviderOptions(string providerName, Action<ExternalProviderOptions> configure)
	{
		if (!_builder.Options.ExternalProviders.TryGetValue(providerName, out var existing))
		{
			existing = new ExternalProviderOptions();
			_builder.Options.ExternalProviders[providerName] = existing;
		}

		configure(existing);
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


