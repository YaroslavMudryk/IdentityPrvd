using IdentityPrvd.Features.Authentication.ExternalSignin.Dtos;
using IdentityPrvd.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace IdentityPrvd.DependencyInjection.Auth.Providers;

public sealed class GoogleProvider : ICustomExternalProvider
{
    public string Provider => "Google";

    public async Task<ExternalUserDto> GetUserFromProviderAsync(AuthenticateResult authResult)
    {
        var provider = authResult.Principal.Identity.AuthenticationType;
        var userId = authResult.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var userFullname = authResult.Principal.FindFirstValue(ClaimTypes.Name);
        var firstName = authResult.Principal.FindFirstValue(ClaimTypes.GivenName);
        var lastName = authResult.Principal.FindFirstValue(ClaimTypes.Surname);
        var email = authResult.Principal.FindFirstValue(ClaimTypes.Email);
        string picture = null;

        if (authResult.Properties?.GetTokenValue("access_token") != null)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.Properties?.GetTokenValue("access_token"));
            var response = await client.GetAsync("https://www.googleapis.com/oauth2/v3/userinfo");
            var userInfoJson = await response.Content.ReadFromJsonAsync<GoogleProfileInfo>();
            picture = userInfoJson.Picture;
        }

        return new ExternalUserDto
        {
            Provider = provider,
            ProviderUserId = userId,
            FullName = userFullname,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Picture = picture,
            Language = null,
            Phone = null,
        };
    }

    public void Register(AuthenticationBuilder authBuilder, IdentityPrvdOptions identityOptions)
    {
        authBuilder.AddGoogle(o =>
        {
            o.ClientId = identityOptions.ExternalProviders[Provider].ClientId;
            o.ClientSecret = identityOptions.ExternalProviders[Provider].ClientSecret;
            o.CallbackPath = "/signin-google";
            o.SignInScheme = "cookie";
            o.SaveTokens = true;
        });
    }
}

public class GoogleProfileInfo
{
    [JsonPropertyName("sub")]
    public string Id { get; set; }
    [JsonPropertyName("name")]
    public string FullName { get; set; }
    [JsonPropertyName("given_name")]
    public string FirstName { get; set; }
    [JsonPropertyName("family_name")]
    public string LastName { get; set; }
    [JsonPropertyName("picture")]
    public string Picture { get; set; }
    [JsonPropertyName("email")]
    public string Email { get; set; }
    [JsonPropertyName("email_verified")]
    public bool EmailVerified { get; set; }
}
