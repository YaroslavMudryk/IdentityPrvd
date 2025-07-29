using IdentityPrvd.Common.Api;
using IdentityPrvd.Endpoints;
using IdentityPrvd.Features.Security.Sessions.GetSessions.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IdentityPrvd.Features.Security.Sessions.GetSessions;

public class GetSessionsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/identity/sessions",
            async (GetSessionsOrchestrator orc) =>
            {
                var result = await orc.GetUserSessionsAsync();
                return Results.Ok(result.MapToResponse());
            }).WithTags("Sessions");
    }
}
