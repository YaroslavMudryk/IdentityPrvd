using IdentityPrvd.Extensions;
using IdentityPrvd.Options;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Services.AuthSchemes;

public class FacebookProviderConfigurator : IExternalProviderConfigurator
{
    public string ProviderName => "Facebook";

    public void Configure(IdentityAuthenticationBuilder builder, ExternalProviderOptions options)
    {
        builder.AuthenticationBuilder.AddFacebook(facebookOptions =>
        {
            facebookOptions.AppId = options.ClientId;
            facebookOptions.AppSecret = options.ClientSecret;
            facebookOptions.CallbackPath = options.CallbackPath ?? "/signin-facebook";
            facebookOptions.SignInScheme = "cookie";
            facebookOptions.SaveTokens = true;

            foreach (var scope in options.Scopes)
            {
                facebookOptions.Scope.Add(scope);
            }
        });
    }
} 