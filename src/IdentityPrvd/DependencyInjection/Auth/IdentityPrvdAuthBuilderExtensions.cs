using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.DependencyInjection.Auth;

public static class IdentityPrvdAuthBuilderExtensions
{
    public static IIdentityPrvdBuilder UseExternalProviders(this IIdentityPrvdBuilder builder, Action<IExternalProvidersBuilder> configure)
    {
        if (configure != null)
        {
            var providersBuilder = new ExternalProvidersBuilder(builder);
            configure(providersBuilder);
        }

        // When using the fluent providers builder, assume explicit registration is done there
        return builder;
    }

    public static IIdentityPrvdBuilder UseExternalProviders(this IIdentityPrvdBuilder builder)
    {
        var identityOptions = builder.Options;
        var authBuilder = builder.AuthenticationBuilder;

        if (identityOptions.ExternalProviders.TryGetValue("Google", out var googleOptions))
        {
            authBuilder.AddGoogle(options =>
            {
                options.ClientId = googleOptions.ClientId;
                options.ClientSecret = googleOptions.ClientSecret;
                options.CallbackPath = string.IsNullOrWhiteSpace(googleOptions.CallbackPath) ? "/signin-google" : googleOptions.CallbackPath;
                options.SignInScheme = "cookie";
                options.SaveTokens = true;
            });
        }

        if (identityOptions.ExternalProviders.TryGetValue("Microsoft", out var microsoftOptions))
        {
            authBuilder.AddMicrosoftAccount(options =>
            {
                options.ClientId = microsoftOptions.ClientId;
                options.ClientSecret = microsoftOptions.ClientSecret;
                options.CallbackPath = string.IsNullOrWhiteSpace(microsoftOptions.CallbackPath) ? "/signin-microsoft" : microsoftOptions.CallbackPath;
                options.SignInScheme = "cookie";
                options.SaveTokens = true;
            });
        }

        if (identityOptions.ExternalProviders.TryGetValue("GitHub", out var githubOptions))
        {
            authBuilder.AddGitHub(options =>
            {
                options.ClientId = githubOptions.ClientId;
                options.ClientSecret = githubOptions.ClientSecret;
                options.CallbackPath = string.IsNullOrWhiteSpace(githubOptions.CallbackPath) ? "/signin-github" : githubOptions.CallbackPath;
                options.SignInScheme = "cookie";
                if (githubOptions.Scopes != null && githubOptions.Scopes.Count > 0)
                {
                    foreach (var scope in githubOptions.Scopes)
                    {
                        options.Scope.Add(scope);
                    }
                }
                else
                {
                    options.Scope.Add("read:user");
                    options.Scope.Add("user:email");
                }
                options.SaveTokens = true;
            });
        }

        if (identityOptions.ExternalProviders.TryGetValue("Facebook", out var facebookOptions))
        {
            authBuilder.AddFacebook(options =>
            {
                options.ClientId = facebookOptions.ClientId;
                options.ClientSecret = facebookOptions.ClientSecret;
                options.CallbackPath = string.IsNullOrWhiteSpace(facebookOptions.CallbackPath) ? "/signin-facebook" : facebookOptions.CallbackPath;
                options.SignInScheme = "cookie";
                options.SaveTokens = true;
            });
        }

        if (identityOptions.ExternalProviders.TryGetValue("Twitter", out var twitterOptions))
        {
            authBuilder.AddTwitter(options =>
            {
                options.ClientId = twitterOptions.ClientId;
                options.ClientSecret = twitterOptions.ClientSecret;
                options.CallbackPath = string.IsNullOrWhiteSpace(twitterOptions.CallbackPath) ? "/signin-twitter" : twitterOptions.CallbackPath;
                options.SignInScheme = "cookie";
                if (twitterOptions.Scopes != null && twitterOptions.Scopes.Count > 0)
                {
                    foreach (var scope in twitterOptions.Scopes)
                    {
                        options.Scope.Add(scope);
                    }
                }
                else
                {
                    options.Scope.Add("users.read");
                    options.Scope.Add("users.email");
                }
                options.SaveTokens = true;
            });
        }

        if (identityOptions.ExternalProviders.TryGetValue("Steam", out var steamOptions))
        {
            authBuilder.AddSteam(options =>
            {
                options.ApplicationKey = steamOptions.ClientId;
                options.CallbackPath = string.IsNullOrWhiteSpace(steamOptions.CallbackPath) ? "/signin-steam" : steamOptions.CallbackPath;
                options.SignInScheme = "cookie";
                options.SaveTokens = true;
            });
        }

        return builder;
    }
}
