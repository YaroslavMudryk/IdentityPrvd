using IdentityPrvd.Common.Api;
using IdentityPrvd.Endpoints;
using IdentityPrvd.Features.Authorization.Roles.Dtos;
using IdentityPrvd.Features.Authorization.Roles.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IdentityPrvd.Features.Authorization.Roles;

public class GetRolesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/identity/roles",
            async (GetRolesOrchestrator orc) =>
            {
                var roles = await orc.GetRolesAsync();
                return Results.Ok(roles.MapToResponse());
            }).WithTags("Roles");
    }
}

public class CreateRoleEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/roles",
            async (CreateRoleDto dto, CreateRoleOrchestrator orc) =>
            {
                var createdRole = await orc.CreateRoleAsync(dto);
                return Results.Json(createdRole.MapToResponse(), statusCode: 201);
            }).WithTags("Roles");
    }
}

public class UpdateRoleEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/identity/roles/{roleId}",
            async (Ulid roleId, UpdateRoleDto dto, UpdateRoleOrchestrator orc) =>
            {
                dto.Id = roleId;
                var updatedRole = await orc.UpdateRoleAsync(roleId, dto);
                return Results.Ok(updatedRole.MapToResponse());
            }).WithTags("Roles");
    }
}

public class DeleteRoleEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/identity/roles/{roleId}",
            async (Ulid roleId, DeleteRoleOrchestrator orc) =>
            {
                await orc.DeleteRoleAsync(roleId);
                return Results.NoContent();
            }).WithTags("Roles");
    }
}
