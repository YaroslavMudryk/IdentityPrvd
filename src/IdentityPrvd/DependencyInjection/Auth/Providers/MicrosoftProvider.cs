using IdentityPrvd.Features.Authentication.ExternalSignin.Dtos;
using IdentityPrvd.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using System.Security.Claims;

namespace IdentityPrvd.DependencyInjection.Auth.Providers;

public static class MicrosoftProviderExtensions
{
    public static IExternalProvidersBuilder AddMicrosoft(this IExternalProvidersBuilder authBuilder)
    {
        return authBuilder.AddCustomProvider<MicrosoftProvider>();
    }
}

public sealed class MicrosoftProvider : ICustomExternalProvider
{
    public string Provider => "Microsoft";

    public async Task<ExternalUserDto> GetUserFromProviderAsync(AuthenticateResult authResult)
    {
        var provider = authResult.Principal.Identity.AuthenticationType;
        var userId = authResult.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var userFullname = authResult.Principal.FindFirstValue(ClaimTypes.Name);
        var firstName = authResult.Principal.FindFirstValue(ClaimTypes.GivenName);
        var lastName = authResult.Principal.FindFirstValue(ClaimTypes.Surname);
        var email = authResult.Principal.FindFirstValue(ClaimTypes.Email);
        string language = null;

        var token = authResult.Properties?.GetTokenValue("access_token");
        if (token != null)
        {
            var client = new GraphServiceClient(new BearerTokenCredential(token));
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

    public void Register(AuthenticationBuilder authBuilder, IdentityPrvdOptions identityOptions)
    {
        authBuilder.AddMicrosoftAccount(o =>
        {
            var providerOptions = identityOptions.ExternalProviders[Provider];
            o.ClientId = providerOptions.ClientId;
            o.ClientSecret = providerOptions.ClientSecret;
            o.CallbackPath = "/signin-microsoft";
            o.SignInScheme = "cookie";
            o.SaveTokens = true;
        });
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
