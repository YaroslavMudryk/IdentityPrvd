using IdentityPrvd.Common.Api;
using IdentityPrvd.Endpoints;
using IdentityPrvd.Features.Security.Initialize.Dtos;
using IdentityPrvd.Features.Security.Initialize.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IdentityPrvd.Features.Security.Initialize;

public class InitializeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/system/initialize",
            [AllowAnonymous] async (InitializeRequestDto dto, InitializeOrchestrator orc) =>
            {
                var initializeResponse = await orc.InitializeAsync(dto);
                return Results.Ok(initializeResponse.MapToResponse());
            }).WithTags("Initialize");
    }
}
