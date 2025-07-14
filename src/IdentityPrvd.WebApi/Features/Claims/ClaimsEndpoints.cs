using IdentityPrvd.WebApi.Api;
using IdentityPrvd.WebApi.Features.Claims.Dtos;
using IdentityPrvd.WebApi.Features.Claims.Services;

namespace IdentityPrvd.WebApi.Features.Claims;

public class GetClaimsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/identity/claims",
            async (GetClaimsOrchestrator orc) =>
            {
                var claims = await orc.GetClaimsAsync();
                return Results.Ok(claims.MapToResponse());
            }).WithTags("Claims");
    }
}

public class CreateClaimEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/claims",
            async (CreateClaimDto dto, CreateClaimOrchestrator orc) =>
            {
                var createdClaim = await orc.CreateClaimAsync(dto);
                return Results.Json(createdClaim.MapToResponse(), statusCode: 201);
            }).WithTags("Claims");
    }
}

public class UpdateClaimEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/identity/claims/{claimId}",
            async (Ulid claimId, UpdateClaimDto dto, UpdateClaimOrchestrator orc) =>
            {
                dto.Id = claimId;
                var updatedClaim = await orc.UpdateClaimAsync(claimId, dto);
                return Results.Ok(updatedClaim.MapToResponse());
            }).WithTags("Claims");
    }
}

public class DeleteClaimEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/identity/claims/{claimId}",
            async (Ulid claimId, DeleteClaimOrchestrator orc) =>
            {
                await orc.DeleteClaimAsync(claimId);
                return Results.NoContent();
            }).WithTags("Claims");
    }
}
