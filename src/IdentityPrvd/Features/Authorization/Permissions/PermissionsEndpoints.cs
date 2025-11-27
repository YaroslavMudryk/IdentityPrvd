using IdentityPrvd.Common.Api;
using IdentityPrvd.Endpoints;
using IdentityPrvd.Features.Authorization.Permissions.Dtos;
using IdentityPrvd.Features.Authorization.Permissions.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IdentityPrvd.Features.Authorization.Permissions;

public class GetPermissionsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/identity/permissions",
            async (GetPermissionsOrchestrator orc) =>
            {
                var permissions = await orc.GetPermissionsAsync();
                return Results.Ok(permissions.MapToResponse());
            }).WithTags("Permissions");
    }
}

public class CreatePermissionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/permissions",
            async (CreatePermissionDto dto, CreatePermissionOrchestrator orc) =>
            {
                var createdPermission = await orc.CreatePermissionAsync(dto);
                return Results.Json(createdPermission.MapToResponse(), statusCode: 201);
            }).WithTags("Permissions");
    }
}

public class UpdatePermissionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/identity/permissions/{permissionId:guid}",
            async (Guid permissionId, UpdatePermissionDto dto, UpdatePermissionOrchestrator orc) =>
            {
                dto.Id = permissionId;
                var updatedPermission = await orc.UpdatePermissionAsync(permissionId, dto);
                return Results.Ok(updatedPermission.MapToResponse());
            }).WithTags("Permissions");
    }
}

public class DeletePermissionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/identity/permissions/{permissionId:guid}",
            async (Guid permissionId, DeletePermissionOrchestrator orc) =>
            {
                await orc.DeletePermissionAsync(permissionId);
                return Results.NoContent();
            }).WithTags("Permissions");
    }
}
