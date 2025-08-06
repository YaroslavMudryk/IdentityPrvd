using IdentityPrvd.Extensions;
using IdentityPrvd.Options;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Services.AuthSchemes;

public class SteamProviderConfigurator : IExternalProviderConfigurator
{
    public string ProviderName => "Steam";

    public void Configure(IdentityAuthenticationBuilder builder, ExternalProviderOptions options)
    {
        builder.AuthenticationBuilder.AddSteam(steamOptions =>
        {
            steamOptions.ApplicationKey = options.ClientId;
            steamOptions.CallbackPath = options.CallbackPath ?? "/signin-steam";
            steamOptions.SignInScheme = "cookie";
            steamOptions.SaveTokens = true;
        });
    }
} 