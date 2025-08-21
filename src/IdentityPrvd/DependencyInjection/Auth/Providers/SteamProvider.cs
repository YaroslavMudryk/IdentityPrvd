using IdentityPrvd.Features.Authentication.ExternalSignin.Dtos;
using IdentityPrvd.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace IdentityPrvd.DependencyInjection.Auth.Providers;

public static class SteamProviderExtensions
{
    public static IExternalProvidersBuilder AddSteam(this IExternalProvidersBuilder authBuilder)
    {
        return authBuilder.AddCustomProvider<SteamProvider>();
    }
}

public sealed class SteamProvider : ICustomExternalProvider
{
    public string Provider => "Steam";

    public async Task<ExternalUserDto> GetUserFromProviderAsync(AuthenticateResult authResult)
    {
        var provider = authResult.Principal.Identity.AuthenticationType;
        var steamUserId = authResult.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = steamUserId.Split('/').Last();
        var userName = authResult.Principal.FindFirstValue(ClaimTypes.Name);

        var token = authResult.Properties?.GetTokenValue("access_token");
        if (token != null)
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"https://api.steampowered.com/ISteamUserOAuth/GetTokenDetails/v1/?access_token={token}");
            var json = await response.Content.ReadAsStringAsync();
        }

        return new ExternalUserDto
        {
            Provider = provider,
            ProviderUserId = userId,
            UserName = userName
        };
    }

    public void Register(AuthenticationBuilder authBuilder, IdentityPrvdOptions identityOptions)
    {
        authBuilder.AddSteam(o =>
        {
            o.ApplicationKey = identityOptions.ExternalProviders[Provider].ClientId;
            o.CallbackPath = "/signin-steam";
            o.SignInScheme = "cookie";
            o.SaveTokens = true;
        });
    }
}
