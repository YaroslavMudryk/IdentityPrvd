using IdentityPrvd.Common.Api;
using IdentityPrvd.Common.Extensions;
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
        app.MapDelete("/api/identity/sessions/revoke",
            async (string[] sessionIds, RevokeSessionsOrchestrator orc) =>
            {
                var revokeSessionsCount = await orc.RevokeSessionsAsync([.. sessionIds.Select(s => s.GetIdAsGuid())]);
                return Results.Ok(revokeSessionsCount.MapToResponse());
            }).WithTags("Sessions");
    }
}

public class DeleteCurrentSessionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/identity/sessions/revoke/current",
            async (RevokeSessionsOrchestrator orc) =>
            {
                await orc.RevokeCurrentSessionAsync();
                return Results.NoContent();
            }).WithTags("Sessions");
    }
}
