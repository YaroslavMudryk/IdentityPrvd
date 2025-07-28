using IdentityPrvd.Features.Security.Sessions.GetSessions.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Features.Security.Sessions.GetSessions;

public static class GetSessionsDependencies
{
    public static IServiceCollection AddGetSessionsDependencies(this IServiceCollection services)
    {
        services.AddScoped<GetSessionsOrchestrator>();
        return services;
    }
}
