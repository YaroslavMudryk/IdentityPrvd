using IdentityPrvd.Features.Authentication.SigninOptions.Dtos;
using IdentityPrvd.Options;
using Microsoft.AspNetCore.Authentication;

namespace IdentityPrvd.Features.Authentication.SigninOptions.Services;

public class AuthenticationProviderDiscoveryService(IAuthenticationSchemeProvider authenticationSchemeProvider, IdentityPrvdOptions identityOptions) : IAuthenticationProviderDiscoveryService
{
    public async Task<SigninOptionsDto> GetAvailableSigninOptionsAsync()
    {
        var schemes = await authenticationSchemeProvider.GetAllSchemesAsync();

        return new SigninOptionsDto
        {
            PasswordSignin = true,
            ExternalProviders = [.. schemes.Where(scheme =>
            {
                if (scheme.Name == "Bearer" || scheme.Name == "cookie" || scheme.Name == "Identity.Application")
                    return false;

                if (IsProviderAvailable(scheme.Name))
                    return true;

                return false;
            }).Select(s => s.Name)]
        };
    }

    private bool IsProviderAvailable(string schemeName)
    {
        // Check if the provider is configured and available in our options
        return identityOptions.ExternalProviders.ContainsKey(schemeName) &&
               identityOptions.ExternalProviders[schemeName].IsAvailable;
    }
}
