using IdentityPrvd.Domain.ValueObjects;

namespace IdentityPrvd.Features.Authentication.Signin.Dtos;

public class SigninRequestDto
{
    public string Mode { get; set; } = SigninModes.Password;

    public string Login { get; set; }
    public string Password { get; set; }
    public string Code { get; set; }
    public string VerificationId { get; set; }

    public string Language { get; set; } = "en";
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string AppVersion { get; set; }
    public Dictionary<string, string> Data { get; set; }
    public ClientInfo Client { get; set; }

    public bool TryGetMode(out SigninMode mode)
    {
        switch ((Mode ?? SigninModes.Password).Trim().ToLowerInvariant())
        {
            case SigninModes.Password:
                mode = SigninMode.Password;
                return true;
            case SigninModes.Passwordless:
                mode = SigninMode.Passwordless;
                return true;
            case SigninModes.Mfa:
                mode = SigninMode.Mfa;
                return true;
            default:
                mode = SigninMode.Password;
                return false;
        }
    }

    public PasswordSigninDto ToPasswordDto()
    {
        return new PasswordSigninDto
        {
            Login = Login,
            Password = Password,
            Language = Language,
            ClientId = ClientId,
            ClientSecret = ClientSecret,
            AppVersion = AppVersion,
            Data = Data,
            Client = Client
        };
    }

    public PasswordlessSigninDto ToPasswordlessDto()
    {
        return new PasswordlessSigninDto
        {
            Login = Login,
            Code = Code,
            Language = Language,
            ClientId = ClientId,
            ClientSecret = ClientSecret,
            AppVersion = AppVersion,
            Data = Data,
            Client = Client
        };
    }

    public MfaSigninDto ToMfaDto()
    {
        return new MfaSigninDto
        {
            VerificationId = VerificationId,
            Code = Code
        };
    }
}
