using IdentityPrvd.Features.Authentication.ChangeLogin.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Features.Authentication.ChangeLogin;

public static class ChangeLoginDependencies
{
    public static IServiceCollection AddChangeLoginDependencies(this IServiceCollection services)
    {
        services.AddScoped<ChangeLoginOrchestrator>();
        return services;
    }
}
