using IdentityPrvd.Options;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Services.AuthSchemes;

public class MicrosoftProviderConfigurator : IExternalProviderConfigurator
{
    public string ProviderName => "Microsoft";

    public void Configure(IdentityAuthenticationBuilder builder, ExternalProviderOptions options)
    {
        builder.AuthenticationBuilder.AddMicrosoftAccount(microsoftOptions =>
        {
            microsoftOptions.ClientId = options.ClientId;
            microsoftOptions.ClientSecret = options.ClientSecret;
            microsoftOptions.CallbackPath = options.CallbackPath ?? "/signin-microsoft";
            microsoftOptions.SignInScheme = "cookie";
            microsoftOptions.SaveTokens = true;

            foreach (var scope in options.Scopes)
            {
                microsoftOptions.Scope.Add(scope);
            }
        });
    }
} 