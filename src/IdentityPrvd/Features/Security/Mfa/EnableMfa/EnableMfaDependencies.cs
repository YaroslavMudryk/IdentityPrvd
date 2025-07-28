using IdentityPrvd.Features.Security.Mfa.EnableMfa.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Features.Security.Mfa.EnableMfa;

public static class EnableMfaDependencies
{
    public static IServiceCollection AddEnableMfaDependencies(this IServiceCollection services)
    {
        services.AddScoped<EnableMfaOrchestrator>();
        return services;
    }
}
