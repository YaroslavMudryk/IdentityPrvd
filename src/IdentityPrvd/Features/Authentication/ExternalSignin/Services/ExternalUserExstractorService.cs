using AspNet.Security.OAuth.GitHub;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Features.Authentication.ExternalSignin.Dtos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Graph;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;

namespace IdentityPrvd.Features.Authentication.ExternalSignin.Services;

public class ExternalUserExstractorService
{
    public async Task<ExternalUserDto> GetUserFromExternalProviderAsync(AuthenticateResult authResult)
    {
        var provider = authResult.Principal.Identity.AuthenticationType;
        return provider switch
        {
            "Google" => await GetUserFromGoogleAsync(authResult),
            "Microsoft" => await GetUserFromMicrosoftAsync(authResult),
            "GitHub" => await GetUserFromGitHubAsync(authResult),
            "Facebook" => await GetUserFromFacebookAsync(authResult),
            "Twitter" => await GetUserFromTwitterAsync(authResult),
            "Steam" => await GetUserFromSteamAsync(authResult),
            _ => throw new BadRequestException($"Unsupported provider: {provider}"),
        };
    }

    private static async Task<ExternalUserDto> GetUserFromGoogleAsync(AuthenticateResult authResult)
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

    private static async Task<ExternalUserDto> GetUserFromMicrosoftAsync(AuthenticateResult authResult)
    {
        var provider = authResult.Principal.Identity.AuthenticationType;
        var userId = authResult.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var userFullname = authResult.Principal.FindFirstValue(ClaimTypes.Name);
        var firstName = authResult.Principal.FindFirstValue(ClaimTypes.GivenName);
        var lastName = authResult.Principal.FindFirstValue(ClaimTypes.Surname);
        var email = authResult.Principal.FindFirstValue(ClaimTypes.Email);
        string language = null;

        if (authResult.Properties?.GetTokenValue("access_token") != null)
        {
            var client = new GraphServiceClient(new BearerTokenCredential(authResult.Properties.GetTokenValue("access_token")));
            var meInfo = await client.Me.GetAsync();
            language = meInfo.PreferredLanguage;
        }

        return new ExternalUserDto
        {
            Provider = provider,
            ProviderUserId = userId,
            FullName = userFullname,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Language = language,
            Phone = null,
            Picture = null
        };
    }

    private static async Task<ExternalUserDto> GetUserFromGitHubAsync(AuthenticateResult authResult)
    {
        var provider = authResult.Principal.Identity.AuthenticationType;
        var userId = authResult.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var userName = authResult.Principal.FindFirstValue(ClaimTypes.Name);
        var fullName = authResult.Principal.FindFirstValue(GitHubAuthenticationConstants.Claims.Name);
        var email = authResult.Principal.FindFirstValue(ClaimTypes.Email);
        string picture = null;

        if (authResult.Properties?.GetTokenValue("access_token") != null)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.Properties?.GetTokenValue("access_token"));
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("IdentityPrvd", "1.0"));
            using var response = await httpClient.GetAsync(GitHubAuthenticationDefaults.UserInformationEndpoint);
            var userInfo = await response.Content.ReadFromJsonAsync<GitHubProfileInfo>();
            picture = userInfo.AvatarUrl;
        }

        return new ExternalUserDto
        {
            Provider = provider,
            ProviderUserId = userId,
            UserName = userName,
            FullName = fullName,
            Email = email,
            Picture = picture,
            FirstName = null,
            LastName = null,
            Phone = null,
            Language = null
        };
    }

    private static async Task<ExternalUserDto> GetUserFromFacebookAsync(AuthenticateResult authResult)
    {
        var provider = authResult.Principal.Identity.AuthenticationType;
        var userId = authResult.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var fullName = authResult.Principal.FindFirstValue(ClaimTypes.Name);
        var firstName = authResult.Principal.FindFirstValue(ClaimTypes.GivenName);
        var lastName = authResult.Principal.FindFirstValue(ClaimTypes.Surname);
        var email = authResult.Principal.FindFirstValue(ClaimTypes.Email);
        string picture = null;

        if (authResult.Properties?.GetTokenValue("access_token") != null)
        {
            using var client = new HttpClient();
            var response = await client.GetAsync($"https://graph.facebook.com/v20.0/me?fields=id,name,email,picture,first_name,last_name,languages&access_token={authResult.Properties?.GetTokenValue("access_token")}");
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

    private static async Task<ExternalUserDto> GetUserFromTwitterAsync(AuthenticateResult authResult)
    {
        return await Task.FromResult(new ExternalUserDto
        {

        });
    }

    private static async Task<ExternalUserDto> GetUserFromSteamAsync(AuthenticateResult authResult)
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
}

public class BearerTokenCredential(string accessToken) : IAuthenticationProvider
{
    public Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object> additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
    {
        request.Headers.TryAdd("Authorization", $"Bearer {accessToken}");
        return Task.CompletedTask;
    }
}
