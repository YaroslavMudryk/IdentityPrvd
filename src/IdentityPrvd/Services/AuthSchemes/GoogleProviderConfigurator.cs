using IdentityPrvd.Options;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Services.AuthSchemes;

public class GoogleProviderConfigurator : IExternalProviderConfigurator
{
    public string ProviderName => "Google";

    public void Configure(IdentityAuthenticationBuilder builder, ExternalProviderOptions options)
    {
        builder.AuthenticationBuilder.AddGoogle(googleOptions =>
        {
            googleOptions.ClientId = options.ClientId;
            googleOptions.ClientSecret = options.ClientSecret;
            googleOptions.CallbackPath = options.CallbackPath ?? "/signin-google";
            googleOptions.SignInScheme = "cookie";
            googleOptions.SaveTokens = true;

            foreach (var scope in options.Scopes)
            {
                googleOptions.Scope.Add(scope);
            }
        });
    }
} 