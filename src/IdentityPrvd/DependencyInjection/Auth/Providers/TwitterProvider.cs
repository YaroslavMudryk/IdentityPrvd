using AspNet.Security.OAuth.Twitter;
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

public static class TwitterProviderExtensions
{
    public static IExternalProvidersBuilder AddTwitter(this IExternalProvidersBuilder authBuilder)
    {
        return authBuilder.AddCustomProvider<TwitterProvider>();
    }
}

public sealed class TwitterProvider : ICustomExternalProvider
{
    public string Provider => "Twitter";

    public async Task<ExternalUserDto> GetUserFromProviderAsync(AuthenticateResult authResult)
    {
        var provider = authResult.Principal.Identity.AuthenticationType;
        var userId = authResult.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = authResult.Principal.FindFirstValue(ClaimTypes.Email);

        var token = authResult.Properties?.GetTokenValue("access_token");
        if (token != null)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await httpClient.GetAsync(TwitterAuthenticationDefaults.UserInformationEndpoint);
            var userInfo = await response.Content.ReadFromJsonAsync<TwitterProfileInfo>();
        }

        return new ExternalUserDto
        {
            Provider = provider,
            ProviderUserId = userId,
            UserName = email,
            Email = email,
        };
    }

    public void Register(AuthenticationBuilder authBuilder, IdentityPrvdOptions identityOptions)
    {
        authBuilder.AddTwitter(o =>
        {
            var providerOptions = identityOptions.ExternalProviders[Provider];
            o.ClientId = providerOptions.ClientId;
            o.ClientSecret = providerOptions.ClientSecret;
            o.CallbackPath = "/signin-twitter";
            o.SignInScheme = AppConstants.DefaultExternalProviderScheme;
            if (providerOptions.Scopes != null && providerOptions.Scopes.Count > 0)
            {
                foreach (var scope in providerOptions.Scopes)
                    o.Scope.Add(scope);
            }
            else
            {
                o.Scope.Add("users.read");
                o.Scope.Add("users.email");
            }
            o.SaveTokens = true;
        });
    }
}

public class TwitterProfileInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
}
