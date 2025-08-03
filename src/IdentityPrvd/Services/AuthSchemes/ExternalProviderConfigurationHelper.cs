using IdentityPrvd.Options;

namespace IdentityPrvd.Services.AuthSchemes;

public static class ExternalProviderConfigurationHelper
{
    public static IdentityPrvdOptions ConfigureDefaultProviders(this IdentityPrvdOptions options)
    {
        // Configure Google
        if (!options.ExternalProviders.ContainsKey("Google"))
        {
            options.ExternalProviders["Google"] = new ExternalProviderOptions
            {
                AuthenticationScheme = "Google",
                CallbackPath = "/signin-google",
                Icon = "google-icon",
                IsAvailable = false
            };
        }

        // Configure Microsoft
        if (!options.ExternalProviders.ContainsKey("Microsoft"))
        {
            options.ExternalProviders["Microsoft"] = new ExternalProviderOptions
            {
                AuthenticationScheme = "Microsoft",
                CallbackPath = "/signin-microsoft",
                Icon = "microsoft-icon",
                IsAvailable = false
            };
        }

        // Configure GitHub
        if (!options.ExternalProviders.ContainsKey("GitHub"))
        {
            options.ExternalProviders["GitHub"] = new ExternalProviderOptions
            {
                AuthenticationScheme = "GitHub",
                CallbackPath = "/signin-github",
                Icon = "github-icon",
                IsAvailable = false,
                Scopes = ["read:user", "user:email"]
            };
        }

        // Configure Facebook
        if (!options.ExternalProviders.ContainsKey("Facebook"))
        {
            options.ExternalProviders["Facebook"] = new ExternalProviderOptions
            {
                AuthenticationScheme = "Facebook",
                CallbackPath = "/signin-facebook",
                Icon = "facebook-icon",
                IsAvailable = false
            };
        }

        // Configure Twitter
        if (!options.ExternalProviders.ContainsKey("Twitter"))
        {
            options.ExternalProviders["Twitter"] = new ExternalProviderOptions
            {
                AuthenticationScheme = "Twitter",
                CallbackPath = "/signin-twitter",
                Icon = "twitter-icon",
                IsAvailable = false,
                Scopes = ["users.read", "users.email"]
            };
        }

        // Configure Steam
        if (!options.ExternalProviders.ContainsKey("Steam"))
        {
            options.ExternalProviders["Steam"] = new ExternalProviderOptions
            {
                AuthenticationScheme = "Steam",
                CallbackPath = "/signin-steam",
                Icon = "steam-icon",
                IsAvailable = false
            };
        }

        return options;
    }

    public static IdentityPrvdOptions EnableProvider(this IdentityPrvdOptions options, string providerName, string clientId, string clientSecret)
    {
        if (options.ExternalProviders.TryGetValue(providerName, out var provider))
        {
            provider.IsAvailable = true;
            provider.ClientId = clientId;
            provider.ClientSecret = clientSecret;
        }

        return options;
    }

    public static IdentityPrvdOptions DisableProvider(this IdentityPrvdOptions options, string providerName)
    {
        if (options.ExternalProviders.TryGetValue(providerName, out var provider))
        {
            provider.IsAvailable = false;
        }

        return options;
    }
} 