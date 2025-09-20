using IdentityPrvd.Features.Security.Initialize.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Features.Security.Initialize;

public static class InitializeDependencies
{
    public static IServiceCollection AddInitializeDependencies(this IServiceCollection services)
    {
        services.AddScoped<InitializeOrchestrator>();
        return services;
    }
}
