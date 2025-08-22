using IdentityPrvd.Features.Authentication.ExternalSignin.Dtos;
using IdentityPrvd.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace IdentityPrvd.DependencyInjection.Auth.Providers;

public static class AppleProviderExtensions
{
    public static IExternalProvidersBuilder AddApple(this IExternalProvidersBuilder authBuilder)
    {
        return authBuilder.AddCustomProvider<AppleProvider>();
    }
}

public sealed class AppleProvider : ICustomExternalProvider
{
    public string Provider => "Apple";

    public async Task<ExternalUserDto> GetUserFromProviderAsync(AuthenticateResult authResult)
    {
        var provider = authResult.Principal.Identity.AuthenticationType;
        var userId = authResult.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var userName = authResult.Principal.FindFirstValue(ClaimTypes.Name);
        var emailAddress = authResult.Principal.FindFirstValue(ClaimTypes.Email);

        var token = authResult.Properties?.GetTokenValue("access_token");
        if (token != null)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync("https://appleid.apple.com/auth/me");
            var userInfoJson = await response.Content.ReadFromJsonAsync<AppleProfileInfo>();
        }

        return new ExternalUserDto
        {
            Provider = provider,
            ProviderUserId = userId,
            UserName = userName,
            Email = emailAddress,
        };
    }

    public void Register(AuthenticationBuilder authBuilder, IdentityPrvdOptions identityOptions)
    {
        IServiceProvider builder = authBuilder.Services.BuildServiceProvider();
        var webHostEnvironment = builder.GetRequiredService<IWebHostEnvironment>();

        authBuilder.AddApple(o =>
        {
            var providerOptions = identityOptions.ExternalProviders[Provider] as AppleExternalProviderOptions;
            o.ClientId = providerOptions.ClientId;
            o.KeyId = providerOptions.KeyId;
            o.TeamId = providerOptions.TeamId;
            o.UsePrivateKey(key => webHostEnvironment.ContentRootFileProvider.GetFileInfo($"AuthKey_{providerOptions.KeyId}.p8"));
            o.CallbackPath = "/signin-apple";
            o.SignInScheme = "cookie";
            o.SaveTokens = true;
        });
    }
}

public class AppleProfileInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("email")]
    public string Email { get; set; }
}
