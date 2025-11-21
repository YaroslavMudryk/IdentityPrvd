using IdentityPrvd.Features.Authentication.Signup.Dtos;

namespace IdentityPrvd.Tests.Features.Signup;

public class SignupBuilder
{
    private SignupRequestDto _dto = new();

    public static SignupBuilder NewDefaultBuilder() => new();

    public SignupRequestDto Build() => _dto;

    public SignupBuilder With(Action<SignupRequestDto> props)
    {
        props(_dto);
        return this;
    }
}
