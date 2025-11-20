using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Features.Authentication.Signin.Dtos;
using IdentityPrvd.Features.Shared.Dtos;

namespace IdentityPrvd.Features.Authentication.Signin.Services;

public class SigninOrchestrator(
    PasswordSigninService passwordService,
    PasswordlessSigninService passwordlessSigninService,
    MfaSigninService signinMfaService)
{
    public async Task<SigninResponseDto> SigninAsync(SigninRequestDto dto)
    {
        if (!dto.TryGetMode(out var mode))
            throw new BadRequestException("Unsupported signin mode");

        return mode switch
        {
            SigninMode.Password => await passwordService.SigninAsync(dto.ToPasswordDto()),
            SigninMode.Passwordless => await passwordlessSigninService.SigninAsync(dto.ToPasswordlessDto()),
            SigninMode.Mfa => await signinMfaService.SinginMfaAsync(dto.ToMfaDto()),
            _ => throw new BadRequestException("Unsupported signin mode")
        };
    }
}
