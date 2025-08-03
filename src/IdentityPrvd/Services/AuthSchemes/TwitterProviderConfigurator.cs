using IdentityPrvd.Options;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Services.AuthSchemes;

public class TwitterProviderConfigurator : IExternalProviderConfigurator
{
    public string ProviderName => "Twitter";

    public void Configure(IdentityAuthenticationBuilder builder, ExternalProviderOptions options)
    {
        builder.AuthenticationBuilder.AddTwitter(twitterOptions =>
        {
            twitterOptions.ClientId = options.ClientId;
            twitterOptions.ClientSecret = options.ClientSecret;
            twitterOptions.CallbackPath = options.CallbackPath ?? "/signin-twitter";
            twitterOptions.SignInScheme = "cookie";
            twitterOptions.SaveTokens = true;

            // Default Twitter scopes
            if (!options.Scopes.Any())
            {
                twitterOptions.Scope.Add("users.read");
                twitterOptions.Scope.Add("users.email");
            }
            else
            {
                foreach (var scope in options.Scopes)
                {
                    twitterOptions.Scope.Add(scope);
                }
            }
        });
    }
} 