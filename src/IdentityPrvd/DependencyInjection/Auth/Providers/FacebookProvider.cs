using IdentityPrvd.Common.Constants;
using IdentityPrvd.Features.Authentication.ExternalSignin.Dtos;
using IdentityPrvd.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace IdentityPrvd.DependencyInjection.Auth.Providers;

public static class FacebookProviderExtensions
{
    public static IExternalProvidersBuilder AddFacebook(this IExternalProvidersBuilder authBuilder)
    {
        return authBuilder.AddCustomProvider<FacebookProvider>();
    }
}

public sealed class FacebookProvider : ICustomExternalProvider
{
    public string Provider => "Facebook";

    public async Task<ExternalUserDto> GetUserFromProviderAsync(AuthenticateResult authResult)
    {
        var provider = authResult.Principal.Identity.AuthenticationType;
        var userId = authResult.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var fullName = authResult.Principal.FindFirstValue(ClaimTypes.Name);
        var firstName = authResult.Principal.FindFirstValue(ClaimTypes.GivenName);
        var lastName = authResult.Principal.FindFirstValue(ClaimTypes.Surname);
        var email = authResult.Principal.FindFirstValue(ClaimTypes.Email);
        string picture = null;

        var token = authResult.Properties?.GetTokenValue("access_token");
        if (token != null)
        {
            using var client = new HttpClient();
            var response = await client.GetAsync($"https://graph.facebook.com/v20.0/me?fields=id,name,email,picture,first_name,last_name,languages&access_token={token}");
            var json = await response.Content.ReadAsStringAsync();
            var userInfo = await response.Content.ReadFromJsonAsync<FacebookProfileInfo>();
            picture = userInfo.Picture.Data.Url;
        }

        return new ExternalUserDto
        {
            Provider = provider,
            ProviderUserId = userId,
            FullName = fullName,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Picture = picture,
            Language = null,
            Phone = null,
            UserName = null
        };
    }

    public void Register(AuthenticationBuilder authBuilder, IdentityPrvdOptions identityOptions)
    {
        authBuilder.AddFacebook(o =>
        {
            var providerOptions = identityOptions.ExternalProviders[Provider];
            o.ClientId = providerOptions.ClientId;
            o.ClientSecret = providerOptions.ClientSecret;
            o.CallbackPath = "/signin-facebook";
            o.SignInScheme = AppConstants.DefaultExternalProvider;
            o.SaveTokens = true;
        });
    }
}

public class FacebookProfileInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("email")]
    public string Email { get; set; }
    [JsonPropertyName("picture")]
    public Picture Picture { get; set; }
    [JsonPropertyName("first_name")]
    public string FirstName { get; set; }
    [JsonPropertyName("last_name")]
    public string LastName { get; set; }
    [JsonPropertyName("languages")]
    public List<Lang> Languages { get; set; }
}

public class Picture
{
    [JsonPropertyName("data")]
    public Data Data { get; set; }
}

public class Data
{
    [JsonPropertyName("height")]
    public int Height { get; set; }
    [JsonPropertyName("is_silhouette")]
    public bool IsSilhouette { get; set; }
    [JsonPropertyName("url")]
    public string Url { get; set; }
    [JsonPropertyName("width")]
    public int Width { get; set; }
}

public class Lang
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
}
