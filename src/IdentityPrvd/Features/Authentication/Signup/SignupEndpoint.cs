using IdentityPrvd.Common.Api;
using IdentityPrvd.Endpoints;
using IdentityPrvd.Features.Authentication.Signup.Dtos;
using IdentityPrvd.Features.Authentication.Signup.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IdentityPrvd.Features.Authentication.Signup;

public class SignupEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/signup",
            [AllowAnonymous] async (SignupRequestDto dto, SignupOrchestrator orc) =>
            {
                var result = await orc.SignupAsync(dto);
                return Results.Json(result.MapToResponse(), statusCode: 201);
            }).WithTags("Signup");
    }
}
