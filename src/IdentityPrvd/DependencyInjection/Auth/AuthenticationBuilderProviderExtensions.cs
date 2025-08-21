using IdentityPrvd.DependencyInjection.Auth;
using IdentityPrvd.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.DependencyInjection.Auth;

public static class AuthenticationBuilderProviderExtensions
{
	//public static AuthenticationBuilder AddGoogle(this AuthenticationBuilder authenticationBuilder, IdentityPrvdOptions options)
	//{
	//	if (!TryGet(options, "Google", out var provider)) return authenticationBuilder;
	//	authenticationBuilder.AddGoogle(o =>
	//	{
	//		o.ClientId = provider.ClientId;
	//		o.ClientSecret = provider.ClientSecret;
	//		o.CallbackPath = string.IsNullOrWhiteSpace(provider.CallbackPath) ? "/signin-google" : provider.CallbackPath;
	//		o.SignInScheme = "cookie";
	//		o.SaveTokens = true;
	//	});
	//	return authenticationBuilder;
	//}

	//public static AuthenticationBuilder AddMicrosoft(this AuthenticationBuilder authenticationBuilder, IdentityPrvdOptions options)
	//{
	//	if (!TryGet(options, "Microsoft", out var provider)) return authenticationBuilder;
	//	authenticationBuilder.AddMicrosoftAccount(o =>
	//	{
	//		o.ClientId = provider.ClientId;
	//		o.ClientSecret = provider.ClientSecret;
	//		o.CallbackPath = string.IsNullOrWhiteSpace(provider.CallbackPath) ? "/signin-microsoft" : provider.CallbackPath;
	//		o.SignInScheme = "cookie";
	//		o.SaveTokens = true;
	//	});
	//	return authenticationBuilder;
	//}

	//public static AuthenticationBuilder AddGitHub(this AuthenticationBuilder authenticationBuilder, IdentityPrvdOptions options)
	//{
	//	if (!TryGet(options, "GitHub", out var provider)) return authenticationBuilder;
	//	authenticationBuilder.AddGitHub(o =>
	//	{
	//		o.ClientId = provider.ClientId;
	//		o.ClientSecret = provider.ClientSecret;
	//		o.CallbackPath = string.IsNullOrWhiteSpace(provider.CallbackPath) ? "/signin-github" : provider.CallbackPath;
	//		o.SignInScheme = "cookie";
	//		if (provider.Scopes != null && provider.Scopes.Count > 0)
	//		{
	//			foreach (var scope in provider.Scopes)
	//				o.Scope.Add(scope);
	//		}
	//		else
	//		{
	//			o.Scope.Add("read:user");
	//			o.Scope.Add("user:email");
	//		}
	//		o.SaveTokens = true;
	//	});
	//	return authenticationBuilder;
	//}

	//public static AuthenticationBuilder AddFacebook(this AuthenticationBuilder authenticationBuilder, IdentityPrvdOptions options)
	//{
	//	if (!TryGet(options, "Facebook", out var provider)) return authenticationBuilder;
	//	authenticationBuilder.AddFacebook(o =>
	//	{
	//		o.ClientId = provider.ClientId;
	//		o.ClientSecret = provider.ClientSecret;
	//		o.CallbackPath = string.IsNullOrWhiteSpace(provider.CallbackPath) ? "/signin-facebook" : provider.CallbackPath;
	//		o.SignInScheme = "cookie";
	//		o.SaveTokens = true;
	//	});
	//	return authenticationBuilder;
	//}

	//public static AuthenticationBuilder AddTwitter(this AuthenticationBuilder authenticationBuilder, IdentityPrvdOptions options)
	//{
	//	if (!TryGet(options, "Twitter", out var provider)) return authenticationBuilder;
	//	authenticationBuilder.AddTwitter(o =>
	//	{
	//		o.ClientId = provider.ClientId;
	//		o.ClientSecret = provider.ClientSecret;
	//		o.CallbackPath = string.IsNullOrWhiteSpace(provider.CallbackPath) ? "/signin-twitter" : provider.CallbackPath;
	//		o.SignInScheme = "cookie";
	//		if (provider.Scopes != null && provider.Scopes.Count > 0)
	//		{
	//			foreach (var scope in provider.Scopes)
	//				o.Scope.Add(scope);
	//		}
	//		else
	//		{
	//			o.Scope.Add("users.read");
	//			o.Scope.Add("users.email");
	//		}
	//		o.SaveTokens = true;
	//	});
	//	return authenticationBuilder;
	//}

	//public static AuthenticationBuilder AddSteam(this AuthenticationBuilder authenticationBuilder, IdentityPrvdOptions options)
	//{
	//	if (!TryGet(options, "Steam", out var provider)) return authenticationBuilder;
	//	authenticationBuilder.AddSteam(o =>
	//	{
	//		o.ApplicationKey = provider.ClientId;
	//		o.CallbackPath = string.IsNullOrWhiteSpace(provider.CallbackPath) ? "/signin-steam" : provider.CallbackPath;
	//		o.SignInScheme = "cookie";
	//		o.SaveTokens = true;
	//	});
	//	return authenticationBuilder;
	//}

	// Generic helper for custom OAuth-based providers
	public static AuthenticationBuilder AddExternalOAuthFromOptions(this AuthenticationBuilder authenticationBuilder, IdentityPrvdOptions options, string providerName, Action<OAuthOptions> configure = null)
	{
		if (!TryGet(options, providerName, out var provider)) return authenticationBuilder;
		authenticationBuilder.AddOAuth(providerName, o =>
		{
			o.ClientId = provider.ClientId;
			o.ClientSecret = provider.ClientSecret;
			o.CallbackPath = string.IsNullOrWhiteSpace(provider.CallbackPath) ? $"/signin-{providerName.ToLower()}" : provider.CallbackPath;
			o.SignInScheme = "cookie";
			if (provider.Scopes != null)
			{
				foreach (var scope in provider.Scopes)
					o.Scope.Add(scope);
			}
			o.SaveTokens = true;
			configure?.Invoke(o);
		});
		return authenticationBuilder;
	}

	private static bool TryGet(IdentityPrvdOptions options, string providerName, out ExternalProviderOptions provider)
	{
		provider = null!;
		if (options?.ExternalProviders == null) return false;
		if (!options.ExternalProviders.TryGetValue(providerName, out var p)) return false;
		if (p == null || !p.IsAvailable) return false;
		provider = p;
		return true;
	}
}
