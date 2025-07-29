using IdentityPrvd.Features.Authentication.Signout.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Features.Authentication.Signout;

public static class SignoutDependencies
{
    public static IServiceCollection AddSignoutDependencies(this IServiceCollection services)
    {
        services.AddScoped<SignoutOrchestrator>();
        return services;
    }
}
