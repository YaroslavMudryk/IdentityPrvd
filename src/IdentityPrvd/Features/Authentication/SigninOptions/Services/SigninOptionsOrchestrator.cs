using IdentityPrvd.Features.Authentication.SigninOptions.Dtos;
using IdentityPrvd.Services.AuthSchemes;

namespace IdentityPrvd.Features.Authentication.SigninOptions.Services;

public class SigninOptionsOrchestrator(
    IAuthSchemes authSchemes)
{
    public async Task<SigninOptionsDto> GetSigninOptionsAsync()
    {
        var schemes = await authSchemes.GetAvailableSchemesAsync();

        return new SigninOptionsDto
        {
            Password = true,
            Passwordless = true,
            ExternalProviders = [.. schemes.Select(s => s.Provider)]
        };
    }
}
