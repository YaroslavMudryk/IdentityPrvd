using IdentityPrvd.Features.Authentication.SigninOptions.Dtos;

namespace IdentityPrvd.Features.Authentication.SigninOptions.Services;

public class SigninOptionsOrchestrator(
    IAuthenticationProviderDiscoveryService discoveryService)
{
    public async Task<SigninOptionsDto> GetSigninOptionsAsync()
    {
        return await discoveryService.GetAvailableSigninOptionsAsync();
    }
}
