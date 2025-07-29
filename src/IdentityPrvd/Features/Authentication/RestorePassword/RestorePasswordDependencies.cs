using IdentityPrvd.Features.Authentication.RestorePassword.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Features.Authentication.RestorePassword;

public static class RestorePasswordDependencies
{
    public static IServiceCollection AddRestorePasswordDependencies(this IServiceCollection services)
    {
        services.AddScoped<StartRestorePasswordOrchestrator>();
        services.AddScoped<RestorePasswordOrchestrator>();
        return services;
    }
}
