using IdentityPrvd.WebApi.Api;
using IdentityPrvd.WebApi.Features.Roles.Dtos;
using IdentityPrvd.WebApi.Features.Roles.Services;

namespace IdentityPrvd.WebApi.Features.Roles;

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
