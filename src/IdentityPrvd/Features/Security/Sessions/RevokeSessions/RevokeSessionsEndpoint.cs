using IdentityPrvd.Common.Api;
using IdentityPrvd.Endpoints;
using IdentityPrvd.Features.Security.Sessions.RevokeSessions.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IdentityPrvd.Features.Security.Sessions.RevokeSessions;

public class RevokeSessionsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/identity/revoke-sessions",
            async (Ulid[] sessionIds, RevokeSessionsOrchestrator orc) =>
            {
                var revokeSessionsCount = await orc.RevokeSessionsAsync(sessionIds);
                return Results.Ok(revokeSessionsCount.MapToResponse());
            }).WithTags("Sessions");
    }
}
