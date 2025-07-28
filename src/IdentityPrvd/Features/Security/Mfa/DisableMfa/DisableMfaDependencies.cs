using IdentityPrvd.Features.Security.Mfa.DisableMfa.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Features.Security.Mfa.DisableMfa;

public static class DisableMfaDependencies
{
    public static IServiceCollection AddDisableMfaDependencies(this IServiceCollection services)
    {
        services.AddScoped<DisableMfaOrchestrator>();
        return services;
    }
}
