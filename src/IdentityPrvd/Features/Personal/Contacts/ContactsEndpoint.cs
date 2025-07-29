using IdentityPrvd.Common.Api;
using IdentityPrvd.Endpoints;
using IdentityPrvd.Features.Personal.Contacts.Dtos;
using IdentityPrvd.Features.Personal.Contacts.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IdentityPrvd.Features.Personal.Contacts;

public class GetContactsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/identity/contacts",
            async (GetContactsOrchestrator orc) =>
            {
                var contacts = await orc.GetContactsAsync();
                return Results.Ok(contacts.MapToResponse());
            }).WithTags("Contacts");
    }
}

public class CreateContactsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/contacts",
            async (CreateContactDto dto, CreateContactOrchestrator orc) =>
            {
                var createdContact = await orc.CreateContactAsync(dto);
                return Results.Ok(createdContact.MapToResponse());
            }).WithTags("Contacts");
    }
}

public class DeleteContactsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/identity/contacts/{id}",
            async (Ulid id, DeleteContactOrchestrator orc) =>
            {
                await orc.DeleteContactAsync(id);
                return Results.NoContent();
            }).WithTags("Contacts");
    }
}
