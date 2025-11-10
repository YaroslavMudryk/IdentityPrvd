using IdentityPrvd.Common.Api;
using IdentityPrvd.Endpoints;
using IdentityPrvd.Features.Security.Sessions.GetSession.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IdentityPrvd.Features.Security.Sessions.GetSession;

public class GetSessionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/identity/sessions/{sessionId:Ulid}",
            async (Ulid sessionId, GetSessionOrchestrator orc) =>
            {
                var result = await orc.GetUserSessionAsync(sessionId);
                return Results.Ok(result.MapToResponse());
            }).WithTags("Sessions");
    }
}
