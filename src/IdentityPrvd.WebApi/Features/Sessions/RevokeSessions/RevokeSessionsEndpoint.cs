using IdentityPrvd.WebApi.Api;
using IdentityPrvd.WebApi.Features.Sessions.RevokeSessions.DataAccess;
using IdentityPrvd.WebApi.Features.Sessions.RevokeSessions.Services;

namespace IdentityPrvd.WebApi.Features.Sessions.RevokeSessions;

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

public static class RevokeSessionsDependencies
{
    public static IServiceCollection AddRevokeSessionsDependencies(this IServiceCollection services)
    {
        services.AddScoped<RevokeSessionsOrchestrator>();
        services.AddScoped<SessionRevocationValidator>();
        services.AddScoped<RefreshTokenRepo>();
        services.AddScoped<SessionRepo>();
        return services;
    }
}
