using IdentityPrvd.Features.Authentication.SigninOptions.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Features.Authentication.SigninOptions;

public static class SigninOptionsDependencies
{
    public static IServiceCollection AddSigninOptionsDependencies(this IServiceCollection services)
    {
        services.AddScoped<SigninOptionsOrchestrator>();
        return services;
    }
}
