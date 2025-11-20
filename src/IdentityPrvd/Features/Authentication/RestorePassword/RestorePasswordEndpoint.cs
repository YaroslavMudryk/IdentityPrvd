using IdentityPrvd.Common.Api;
using IdentityPrvd.Endpoints;
using IdentityPrvd.Features.Authentication.RestorePassword.Dtos;
using IdentityPrvd.Features.Authentication.RestorePassword.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IdentityPrvd.Features.Authentication.RestorePassword;

public class CreateRestorePasswordRequestEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/restore-password",
            [AllowAnonymous] async (StartRestorePasswordDto dto, StartRestorePasswordOrchestrator orc) =>
            {
                var startedRestoreDto = await orc.StartRestorePasswordAsync(dto);
                return Results.Ok(startedRestoreDto.MapToResponse());
            }).WithTags("Restore password");
    }
}

public class CompleteRestorePasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/identity/restore-password/{verifyId}",
            [AllowAnonymous] async (string verifyId, RestorePasswordDto dto, RestorePasswordOrchestrator orc) =>
            {
                dto.VerifyId = verifyId;
                await orc.RestorePasswordAsync(dto);
                return Results.NoContent();
            }).WithTags("Restore password");
    }
}
