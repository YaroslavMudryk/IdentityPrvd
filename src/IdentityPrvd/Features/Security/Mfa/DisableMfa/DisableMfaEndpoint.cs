using IdentityPrvd.Endpoints;
using IdentityPrvd.Features.Security.Mfa.DisableMfa.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IdentityPrvd.Features.Security.Mfa.DisableMfa;

public class DisableMfaEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/identity/mfa",
            async (string code, DisableMfaOrchestrator orc) =>
            {
                await orc.DisableMfaAsync(code);
                return Results.NoContent();
            }).WithTags("Mfa");
    }
}
