using FluentValidation;
using IdentityPrvd.Features.Authentication.RestorePassword.Dtos;
using IdentityPrvd.Features.Authentication.RestorePassword.Dtos.Validators;
using IdentityPrvd.Features.Authentication.RestorePassword.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Features.Authentication.RestorePassword;

public static class RestorePasswordDependencies
{
    public static IServiceCollection AddRestorePasswordDependencies(this IServiceCollection services)
    {
        services.AddScoped<StartRestorePasswordOrchestrator>();
        services.AddScoped<RestorePasswordOrchestrator>();
        services.AddScoped<IValidator<RestorePasswordDto> , RestorePasswordDtoValidator>();
        return services;
    }
}
