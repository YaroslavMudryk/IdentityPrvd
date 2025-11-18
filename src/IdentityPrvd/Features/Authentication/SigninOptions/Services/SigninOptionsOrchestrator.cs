using IdentityPrvd.Features.Authentication.SigninOptions.Dtos;
using IdentityPrvd.Options;
using IdentityPrvd.Services.AuthSchemes;

namespace IdentityPrvd.Features.Authentication.SigninOptions.Services;

public class SigninOptionsOrchestrator(
    IAuthSchemes authSchemes,
    IdentityPrvdOptions identityOptions)
{
    public async Task<SigninOptionsDto> GetSigninOptionsAsync()
    {
        var schemes = await authSchemes.GetAvailableSchemesAsync();

        return new SigninOptionsDto
        {
            Password = identityOptions.Signin.Password,
            Passwordless = identityOptions.Signin.Passwordless,
            ExternalProviders = [.. schemes.Select(s => s.Provider)]
        };
    }
}
