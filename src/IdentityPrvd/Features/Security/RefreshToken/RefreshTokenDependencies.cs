using FluentValidation;
using IdentityPrvd.Features.Security.RefreshToken.Dtos;
using IdentityPrvd.Features.Security.RefreshToken.Dtos.Validators;
using IdentityPrvd.Features.Security.RefreshToken.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Features.Security.RefreshToken;

public static class RefreshTokenDependencies
{
    public static IServiceCollection AddRefreshTokenDependencies(this IServiceCollection services)
    {
        services.AddScoped<RefreshTokenOrchestrator>();
        services.AddScoped<IValidator<RefreshTokenDto>, RefreshTokenDtoValidator>();
        return services;
    }
}
