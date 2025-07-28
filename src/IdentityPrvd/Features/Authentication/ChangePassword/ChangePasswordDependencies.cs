using IdentityPrvd.Features.Authentication.ChangePassword.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Features.Authentication.ChangePassword;

public static class ChangePasswordDependencies
{
    public static IServiceCollection AddChangePasswordDependencies(this IServiceCollection services)
    {
        services.AddScoped<ChangePasswordOrchestrator>();
        return services;
    }
}
