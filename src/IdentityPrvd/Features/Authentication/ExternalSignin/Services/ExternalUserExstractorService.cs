using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.DependencyInjection.Auth;
using IdentityPrvd.Features.Authentication.ExternalSignin.Dtos;
using Microsoft.AspNetCore.Authentication;

namespace IdentityPrvd.Features.Authentication.ExternalSignin.Services;

public class ExternalUserExstractorService(IEnumerable<ICustomExternalProvider> externalProviders)
{
    public async Task<ExternalUserDto> GetUserFromExternalProviderAsync(AuthenticateResult authResult)
    {
        var provider = authResult.Principal.Identity.AuthenticationType;

        var customProvider = externalProviders.FirstOrDefault(p => p.Provider.Equals(provider, StringComparison.OrdinalIgnoreCase));
        return customProvider == null
            ? throw new BadRequestException($"Unsupported provider: {provider}")
            : await customProvider.GetUserFromProviderAsync(authResult);
    }
}
