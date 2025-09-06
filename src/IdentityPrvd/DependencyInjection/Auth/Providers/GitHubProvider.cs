using AspNet.Security.OAuth.GitHub;
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

public static class GitHubProviderExtensions
{
    public static IExternalProvidersBuilder AddGitHub(this IExternalProvidersBuilder authBuilder)
    {
        return authBuilder.AddCustomProvider<GitHubProvider>();
    }
}

public sealed class GitHubProvider : ICustomExternalProvider
{
    public string Provider => "GitHub";

    public async Task<ExternalUserDto> GetUserFromProviderAsync(AuthenticateResult authResult)
    {
        var provider = authResult.Principal.Identity.AuthenticationType;
        var userId = authResult.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var userName = authResult.Principal.FindFirstValue(ClaimTypes.Name);
        var fullName = authResult.Principal.FindFirstValue(GitHubAuthenticationConstants.Claims.Name);
        var email = authResult.Principal.FindFirstValue(ClaimTypes.Email);
        string picture = null;

        var token = authResult.Properties?.GetTokenValue("access_token");
        if (token != null)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
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

    public void Register(AuthenticationBuilder authBuilder, IdentityPrvdOptions identityOptions)
    {
        authBuilder.AddGitHub(o =>
        {
            var providerOptions = identityOptions.ExternalProviders[Provider];
            o.ClientId = providerOptions.ClientId;
            o.ClientSecret = providerOptions.ClientSecret;
            o.CallbackPath = "/signin-github";
            o.SignInScheme = AppConstants.DefaultExternalProviderScheme;
            if (providerOptions.Scopes != null && providerOptions.Scopes.Count > 0)
            {
                foreach (var scope in providerOptions.Scopes)
                    o.Scope.Add(scope);
            }
            else
            {
                o.Scope.Add("read:user");
                o.Scope.Add("user:email");
            }
            o.SaveTokens = true;
        });
    }
}

public class GitHubProfileInfo
{
    [JsonPropertyName("login")]
    public string Login { get; set; }
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("node_id")]
    public string NodeId { get; set; }
    [JsonPropertyName("avatar_url")]
    public string AvatarUrl { get; set; }
    [JsonPropertyName("gravatar_id")]
    public string GravatarId { get; set; }
    [JsonPropertyName("url")]
    public string Url { get; set; }
    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; }
    [JsonPropertyName("followers_url")]
    public string FollowersUrl { get; set; }
    [JsonPropertyName("following_url")]
    public string FollowingUrl { get; set; }
    [JsonPropertyName("gists_url")]
    public string GistsUrl { get; set; }
    [JsonPropertyName("starred_url")]
    public string StarredUrl { get; set; }
    [JsonPropertyName("subscriptions_url")]
    public string SubscriptionsUrl { get; set; }
    [JsonPropertyName("organizations_url")]
    public string OrganizationsUrl { get; set; }
    [JsonPropertyName("repos_url")]
    public string ReposUrl { get; set; }
    [JsonPropertyName("events_url")]
    public string EventsUrl { get; set; }
    [JsonPropertyName("received_events_url")]
    public string ReceivedEventsUrl { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("site_admin")]
    public bool SiteAdmin { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("company")]
    public string Company { get; set; }
    [JsonPropertyName("blog")]
    public string Blog { get; set; }
    [JsonPropertyName("location")]
    public string Location { get; set; }
    [JsonPropertyName("email")]
    public string Email { get; set; }
    [JsonPropertyName("hireable")]
    public bool? Hireable { get; set; }
    [JsonPropertyName("bio")]
    public string Bio { get; set; }
    [JsonPropertyName("twitter_username")]
    public string TwitterUsername { get; set; }
    [JsonPropertyName("public_repos")]
    public int PublicRepos { get; set; }
    [JsonPropertyName("public_gists")]
    public int PublicGists { get; set; }
    [JsonPropertyName("followers")]
    public int Followers { get; set; }
    [JsonPropertyName("following")]
    public int Following { get; set; }
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
