using IdentityPrvd.Features.Authentication.Signup.Dtos;
using IdentityPrvd.Tests.IntegrationInfra;

namespace IdentityPrvd.Tests.Features.Signup;

public class SignupEndpoints(HttpClient httpClient)
{
    public async Task<TestApiResponse> Signup(SignupRequestDto dto)
        => await TestApiRequest.Post("/api/identity/signup").WithPayload(dto).SendAsync(httpClient);

    public async Task<TestApiResponse> SignupConfirm(SignupConfirmRequestDto dto)
        => await TestApiRequest.Post("/api/identity/signup/confirm").WithPayload(dto).SendAsync(httpClient);
}
