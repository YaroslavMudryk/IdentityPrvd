using IdentityPrvd.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.DependencyInjection.Auth;

public static class AuthenticationBuilderProviderExtensions
{
    // Generic helper for custom OAuth-based providers
    public static AuthenticationBuilder AddExternalOAuthFromOptions(this AuthenticationBuilder authenticationBuilder, IdentityPrvdOptions options, string providerName, Action<OAuthOptions> configure = null)
    {
        if (!TryGet(options, providerName, out var provider))
            return authenticationBuilder;

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
