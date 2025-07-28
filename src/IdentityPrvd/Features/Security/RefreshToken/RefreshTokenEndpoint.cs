using IdentityPrvd.Common.Api;
using IdentityPrvd.Endpoints;
using IdentityPrvd.Features.Security.RefreshToken.Dtos;
using IdentityPrvd.Features.Security.RefreshToken.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IdentityPrvd.Features.Security.RefreshToken;

public class RefreshTokenEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/refresh-token",
            [AllowAnonymous] async (RefreshTokenDto dto, RefreshTokenOrchestrator orc) =>
            {
                var result = await orc.SigninByRefreshTokenAsync(dto);
                return Results.Ok(result.MapToResponse());
            }).WithTags("Signin");
    }
}
