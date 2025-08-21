using IdentityPrvd.Features.Authentication.ExternalSignin.Dtos;
using IdentityPrvd.Options;
using Microsoft.AspNetCore.Authentication;

namespace IdentityPrvd.DependencyInjection.Auth;

public interface ICustomExternalProvider
{
    string Provider { get; }
    void Register(AuthenticationBuilder authBuilder, IdentityPrvdOptions identityOptions);
    Task<ExternalUserDto> GetUserFromProviderAsync(AuthenticateResult authResult);
}
