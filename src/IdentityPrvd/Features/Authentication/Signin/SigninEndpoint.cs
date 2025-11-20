using IdentityPrvd.Common.Api;
using IdentityPrvd.Endpoints;
using IdentityPrvd.Features.Authentication.Signin.Dtos;
using IdentityPrvd.Features.Authentication.Signin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace IdentityPrvd.Features.Authentication.Signin;

public class SigninEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/signin",
            [AllowAnonymous] async (SigninRequestDto dto, SigninOrchestrator orchestrator) =>
            {
                var response = await orchestrator.SigninAsync(dto);
                return Results.Ok(response.MapToResponse());
            }).WithTags("Signin");
    }
}

public class SigninChallengeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/identity/signin/challenge",
            [AllowAnonymous] async ([FromQuery] string login, SigninOptionsOrchestrator orc) =>
            {
                var result = await orc.GetSigninUserOptionsAsync(login);
                return Results.Ok(result.MapToResponse());
            }).WithTags("Signin");
    }
}
