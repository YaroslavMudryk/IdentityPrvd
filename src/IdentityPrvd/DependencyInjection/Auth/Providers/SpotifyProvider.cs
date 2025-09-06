using IdentityPrvd.Common.Constants;
using IdentityPrvd.Features.Authentication.ExternalSignin.Dtos;
using IdentityPrvd.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace IdentityPrvd.DependencyInjection.Auth.Providers;

public static class SpotifyProviderExtensions
{
    public static IExternalProvidersBuilder AddSpotify(this IExternalProvidersBuilder authBuilder)
    {
        return authBuilder.AddCustomProvider<SpotifyProvider>();
    }
}

public class SpotifyProvider : ICustomExternalProvider
{
    public string Provider => "Spotify";

    public Task<ExternalUserDto> GetUserFromProviderAsync(AuthenticateResult authResult)
    {
        var provider = authResult.Principal.Identity.AuthenticationType;
        var userId = authResult.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var userName = authResult.Principal.FindFirstValue(ClaimTypes.Name);
        var email = authResult.Principal.FindFirstValue(ClaimTypes.Email);
        var country = authResult.Principal.FindFirstValue(ClaimTypes.Country);
        var uri = authResult.Principal.FindFirstValue("urn:spotify:url");
        var picture = authResult.Principal.FindFirstValue("urn:spotify:profilepicture");

        return Task.FromResult(new ExternalUserDto
        {
            Provider = provider,
            ProviderUserId = userId,
            UserName = userName,
            FullName = userName,
            Email = email,
            Picture = picture,
            Language = country,
            FirstName = null,
            LastName = null,
            Phone = null,
            Uri = uri
        });
    }

    public void Register(AuthenticationBuilder authBuilder, IdentityPrvdOptions identityOptions)
    {
        authBuilder.AddSpotify(options =>
        {
            var providerOptions = identityOptions.ExternalProviders[Provider];
            options.ClientId = providerOptions.ClientId;
            options.ClientSecret = providerOptions.ClientSecret;
            options.CallbackPath = "/signin-spotify";
            options.SignInScheme = AppConstants.DefaultExternalProviderScheme;
            options.Scope.Add("user-read-private");
            options.Scope.Add("user-read-email");
            options.SaveTokens = true;
        });
    }
}
