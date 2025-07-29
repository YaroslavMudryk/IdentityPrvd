using IdentityPrvd.Endpoints;
using IdentityPrvd.Features.Authentication.ChangeLogin.Dtos;
using IdentityPrvd.Features.Authentication.ChangeLogin.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IdentityPrvd.Features.Authentication.ChangeLogin;

public class ChangeLoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/change-login",
            async (ChangeLoginDto dto, ChangeLoginOrchestrator orc) =>
            {
                await orc.ChangeLoginAsync(dto);
                return Results.NoContent();
            }).WithTags("Change login");
    }
}
