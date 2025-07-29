using IdentityPrvd.Common.Api;
using IdentityPrvd.Endpoints;
using IdentityPrvd.Features.Authentication.RestorePassword.Dtos;
using IdentityPrvd.Features.Authentication.RestorePassword.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IdentityPrvd.Features.Authentication.RestorePassword;

public class StartRestorePasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/start-restore-password",
            async (StartRestorePasswordDto dto, StartRestorePasswordOrchestrator orc) =>
            {
                var startedRestoreDto = await orc.StartRestorePasswordAsync(dto);
                return Results.Ok(startedRestoreDto.MapToResponse());
            }).WithTags("Restore password");
    }
}

public class RestorePasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/restore-password",
            async (RestorePasswordDto dto, RestorePasswordOrchestrator orc) =>
            {
                await orc.RestorePasswordAsync(dto);
                return Results.NoContent();
            }).WithTags("Restore password");
    }
}
