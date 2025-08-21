using AspNet.Security.OAuth.Discord;
using IdentityPrvd.Features.Authentication.ExternalSignin.Dtos;
using IdentityPrvd.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace IdentityPrvd.DependencyInjection.Auth.Providers;

public static class DiscordProviderExtensions
{
    public static IExternalProvidersBuilder AddDiscord(this IExternalProvidersBuilder authBuilder)
    {
        return authBuilder.AddCustomProvider<DiscordProvider>();
    }
}

public sealed class DiscordProvider : ICustomExternalProvider
{
    public string Provider => "Discord";

    public async Task<ExternalUserDto> GetUserFromProviderAsync(AuthenticateResult authResult)
    {
        var provider = authResult.Principal.Identity.AuthenticationType;
        var userId = authResult.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var userName = authResult.Principal.FindFirstValue(ClaimTypes.Name);
        var emailAddress = authResult.Principal.FindFirstValue(ClaimTypes.Email);
        var fullName = string.Empty;
        var locale = string.Empty;
        var avatarUrl = string.Empty;

        var token = authResult.Properties?.GetTokenValue("access_token");
        if (token != null)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync(DiscordAuthenticationDefaults.UserInformationEndpoint);
            var userInfoJson = await response.Content.ReadFromJsonAsync<DiscordProfileInfo>();
            fullName = userInfoJson.GlobalName;
            locale = userInfoJson.Locale;
            avatarUrl = $"https://cdn.discordapp.com/avatars/{userInfoJson.Id}/{userInfoJson.Avatar}";
        }

        return new ExternalUserDto
        {
            Provider = provider,
            ProviderUserId = userId,
            UserName = userName,
            Email = emailAddress,
            FullName = fullName,
            Language = locale,
            Picture = avatarUrl
        };
    }

    public void Register(AuthenticationBuilder authBuilder, IdentityPrvdOptions identityOptions)
    {
        authBuilder.AddDiscord(o =>
        {
            var providerOptions = identityOptions.ExternalProviders[Provider];
            o.ClientId = providerOptions.ClientId;
            o.ClientSecret = providerOptions.ClientSecret;
            o.CallbackPath = "/signin-discord";
            o.SignInScheme = "cookie";
            o.Scope.Add("identify");
            o.Scope.Add("email");
            o.Scope.Add("openid");
            o.SaveTokens = true;
        });
    }
}

public class DiscordProfileInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("avatar")]
    public string Avatar { get; set; }
    [JsonPropertyName("global_name")]
    public string GlobalName { get; set; }
    [JsonPropertyName("locale")]
    public string Locale { get; set; }
}
