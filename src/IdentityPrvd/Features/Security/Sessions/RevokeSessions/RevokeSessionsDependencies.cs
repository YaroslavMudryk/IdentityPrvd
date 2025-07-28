using IdentityPrvd.Features.Security.Sessions.RevokeSessions.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Features.Security.Sessions.RevokeSessions;

public static class RevokeSessionsDependencies
{
    public static IServiceCollection AddRevokeSessionsDependencies(this IServiceCollection services)
    {
        services.AddScoped<RevokeSessionsOrchestrator>();
        services.AddScoped<SessionRevocationValidator>();
        return services;
    }
}
