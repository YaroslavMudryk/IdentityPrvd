using IdentityPrvd.Features.Authentication.ExternalSignin.Dtos;
using IdentityPrvd.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace IdentityPrvd.DependencyInjection.Auth.Providers;

public sealed class SamsungProvider : ICustomExternalProvider
{
    public string Provider => "Samsung";

    public void Register(AuthenticationBuilder authBuilder, IdentityPrvdOptions identityOptions)
    {
        authBuilder.AddExternalOAuthFromOptions(identityOptions, "Samsung", o =>
        {
            o.AuthorizationEndpoint = "https://account.samsung.com/iam/oauth2/authorize";
            o.TokenEndpoint = "https://account.samsung.com/iam/oauth2/token";
            o.UserInformationEndpoint = "https://api.samsung.com/v1/me";

            o.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
            o.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
            o.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");

            o.Events = new OAuthEvents
            {
                OnCreatingTicket = async context =>
                {
                    using var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                    using var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
                    response.EnsureSuccessStatusCode();
                    using var payload = await System.Text.Json.JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(context.HttpContext.RequestAborted), cancellationToken: context.HttpContext.RequestAborted);
                    context.RunClaimActions(payload.RootElement);
                }
            };
        });
    }

    public async Task<ExternalUserDto> GetUserFromProviderAsync(AuthenticateResult authResult)
    {
        var provider = authResult.Principal.Identity.AuthenticationType;
        var userId = authResult.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var userName = authResult.Principal.FindFirstValue(ClaimTypes.Name);

        if (authResult.Properties?.GetTokenValue("access_token") != null)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.Properties?.GetTokenValue("access_token"));
            var response = await client.GetAsync("https://api.samsung.com/v1/me");
            response.EnsureSuccessStatusCode();
            var userInfo = await response.Content.ReadFromJsonAsync<SamsungProfileInfo>();
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
}

public class SamsungProfileInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("email")]
    public string Email { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("picture")]
    public string Picture { get; set; }
}
