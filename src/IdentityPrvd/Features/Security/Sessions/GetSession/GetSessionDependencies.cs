using IdentityPrvd.Features.Security.Sessions.GetSession.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Features.Security.Sessions.GetSession;

public static class GetSessionDependencies
{
    public static IServiceCollection AddGetSessionDependencies(this IServiceCollection services)
    {
        services.AddScoped<GetSessionOrchestrator>();
        return services;
    }
}
