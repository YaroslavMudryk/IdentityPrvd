using IdentityPrvd.Features.Authentication.SigninOptions.Dtos;

namespace IdentityPrvd.Features.Authentication.SigninOptions.Services;

public interface IAuthenticationProviderDiscoveryService
{
    Task<SigninOptionsDto> GetAvailableSigninOptionsAsync();
}
