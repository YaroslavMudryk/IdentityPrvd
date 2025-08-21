using IdentityPrvd.Features.Authentication.ExternalSignin.Dtos;
using IdentityPrvd.Features.Authentication.ExternalSignin.Services;
using IdentityPrvd.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using System.Security.Claims;

namespace IdentityPrvd.DependencyInjection.Auth.Providers;

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

    public void Register(AuthenticationBuilder authBuilder, IdentityPrvdOptions identityOptions)
    {
        authBuilder.AddMicrosoftAccount(o =>
        {
            o.ClientId = identityOptions.ExternalProviders[Provider].ClientId;
            o.ClientSecret = identityOptions.ExternalProviders[Provider].ClientSecret;
            o.CallbackPath = "/signin-microsoft";
            o.SignInScheme = "cookie";
            o.SaveTokens = true;
        });
    }
}
