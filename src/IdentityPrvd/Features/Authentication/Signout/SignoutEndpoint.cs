using IdentityPrvd.Endpoints;
using IdentityPrvd.Features.Authentication.Signout.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IdentityPrvd.Features.Authentication.Signout;

public class SignoutEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/signout",
            async (SignoutOrchestrator orc, bool everywhere = false) =>
            {
                await orc.SignoutAsync(everywhere);
                return Results.NoContent();
            }).WithTags("Signout");
    }
}
