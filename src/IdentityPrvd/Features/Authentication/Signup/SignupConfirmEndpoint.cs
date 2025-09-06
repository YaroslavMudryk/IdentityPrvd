using IdentityPrvd.Endpoints;
using IdentityPrvd.Features.Authentication.Signup.Dtos;
using IdentityPrvd.Features.Authentication.Signup.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IdentityPrvd.Features.Authentication.Signup;

public class SignupConfirmEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/signup/confirm",
            [AllowAnonymous] async (SignupConfirmRequestDto dto, SignupConfirmOrchestrator orc) =>
            {
                await orc.ConfirmAsync(dto);
                return Results.NoContent();
            }).WithTags("Signup");
    }
}
