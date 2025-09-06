using AspNet.Security.OAuth.BattleNet;
using IdentityPrvd.Common.Constants;
using IdentityPrvd.Features.Authentication.ExternalSignin.Dtos;
using IdentityPrvd.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace IdentityPrvd.DependencyInjection.Auth.Providers;

public static class BattleNetProviderExtensions
{
    public static IExternalProvidersBuilder AddBattleNet(this IExternalProvidersBuilder authBuilder)
    {
        return authBuilder.AddCustomProvider<BattleNetProvider>();
    }
}

public sealed class BattleNetProvider : ICustomExternalProvider
{
    public string Provider => "BattleNet";

    public async Task<ExternalUserDto> GetUserFromProviderAsync(AuthenticateResult authResult)
    {
        var provider = authResult.Principal.Identity.AuthenticationType;
        var userId = authResult.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var userName = authResult.Principal.FindFirstValue(ClaimTypes.Name);

        var token = authResult.Properties?.GetTokenValue("access_token");
        if (token != null)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync(BattleNetAuthenticationDefaults.Unified.UserInformationEndpoint);
            response.EnsureSuccessStatusCode();
            var userInfo = await response.Content.ReadFromJsonAsync<BattleNetProfileInfo>();
        }

        return new ExternalUserDto
        {
            Provider = provider,
            ProviderUserId = userId,
            UserName = userName,
            FullName = null,
            Email = null,
            Picture = null,
            FirstName = null,
            LastName = null,
            Phone = null,
            Language = null
        };
    }

    public void Register(AuthenticationBuilder authBuilder, IdentityPrvdOptions identityOptions)
    {
        authBuilder.AddBattleNet(options =>
        {
            var providerOptions = identityOptions.ExternalProviders[Provider];
            options.ClientId = providerOptions.ClientId;
            options.ClientSecret = providerOptions.ClientSecret;
            options.CallbackPath = "/signin-battlenet";
            options.SignInScheme = AppConstants.DefaultExternalProviderScheme;
            options.SaveTokens = true;
        });
    }
}

public class BattleNetProfileInfo
{
    [JsonPropertyName("sub")]
    public string Subject { get; set; }
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("battletag")]
    public string BattleTag { get; set; }
}
