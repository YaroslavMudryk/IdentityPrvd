using IdentityPrvd.Common.Api;
using IdentityPrvd.Endpoints;
using IdentityPrvd.Features.Authentication.SigninOptions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IdentityPrvd.Features.Authentication.SigninOptions;

public class GetSigninOptionsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/identity/signin-options",
            [AllowAnonymous] async (SigninOptionsOrchestrator orchestrator) =>
            {
                var signinOptions = await orchestrator.GetSigninOptionsAsync();
                return Results.Ok(signinOptions.MapToResponse());
            }).WithTags("Authentication");
    }
}
