using IdentityPrvd.Features.Authentication.ExternalSignin.Dtos;
using IdentityPrvd.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.DependencyInjection.Auth.Providers;

public sealed class TwitterProvider : ICustomExternalProvider
{
    public string Provider => "Twitter";

    public async Task<ExternalUserDto> GetUserFromProviderAsync(AuthenticateResult authResult)
    {
        return await Task.FromResult(new ExternalUserDto
        {

        });
    }

    public void Register(AuthenticationBuilder authBuilder, IdentityPrvdOptions identityOptions)
    {
        authBuilder.AddTwitter(o =>
        {
            o.ClientId = identityOptions.ExternalProviders[Provider].ClientId;
            o.ClientSecret = identityOptions.ExternalProviders[Provider].ClientSecret;
            o.CallbackPath = "/signin-twitter";
            o.SignInScheme = "cookie";
            if (identityOptions.ExternalProviders[Provider].Scopes != null && identityOptions.ExternalProviders[Provider].Scopes.Count > 0)
            {
                foreach (var scope in identityOptions.ExternalProviders[Provider].Scopes)
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
