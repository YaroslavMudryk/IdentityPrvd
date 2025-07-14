using IdentityPrvd.WebApi.Api;
using IdentityPrvd.WebApi.Features.Sessions.GetSessions.DataAccess;
using IdentityPrvd.WebApi.Features.Sessions.GetSessions.Services;

namespace IdentityPrvd.WebApi.Features.Sessions.GetSessions;

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

public static class GetSessionsDependencies
{
    public static IServiceCollection AddGetSessionsDependencies(this IServiceCollection services)
    {
        services.AddScoped<GetSessionsOrchestrator>();
        services.AddScoped<GetSessionsQuery>();
        return services;
    }
}
