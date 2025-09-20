using IdentityPrvd.Common.Api;
using IdentityPrvd.Endpoints;
using IdentityPrvd.Features.Authorization.Clients.Dtos;
using IdentityPrvd.Features.Authorization.Clients.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IdentityPrvd.Features.Authorization.Clients;

public class GetClientsEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/identity/clients",
            async (GetClientsOrchestrator orc) =>
            {
                var clients = await orc.GetClientsAsync();
                return Results.Ok(clients.MapToResponse());
            }).WithTags("Clients");
    }
}

public class GetClientEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/identity/clients/{clientId:Ulid}",
            async (Ulid clientId, GetClientOrchestrator orc) =>
            {
                var clients = await orc.GetClientAsync(clientId);
                return Results.Ok(clients.MapToResponse());
            }).WithTags("Clients");
    }
}

public class CreateClientEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/clients",
            async (CreateClientDto dto, CreateClientOrchestrator orc) =>
            {
                var client = await orc.CreateAsync(dto);
                return Results.Json(client.MapToResponse(), statusCode: 201);
            }).WithTags("Clients");
    }
}

public class UpdateClientEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/identity/clients/{clientId:Ulid}",
            async (Ulid clientId, UpdateClientDto dto, UpdateClientOrchestrator orc) =>
            {
                var client = await orc.UpdateAsync(clientId, dto);
                return Results.Ok(client.MapToResponse());
            }).WithTags("Clients");
    }
}

public class UpdateClientClaimsEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/identity/clients/{clientId:Ulid}/claims",
            async (Ulid clientId, UpdateClientClaimsDto dto, UpdateClientClaimsOrchestrator orc) =>
            {
                var client = await orc.UpdateClaimsAsync(clientId, dto);
                return Results.Ok(client.MapToResponse());
            }).WithTags("Clients");
    }
}

public class DeleteClientEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/identity/clients/{clientId:Ulid}",
            async (Ulid clientId, DeleteClientOrchestrator orc) =>
            {
                await orc.DeleteAsync(clientId);
                return Results.NoContent();
            }).WithTags("Clients");
    }
}
