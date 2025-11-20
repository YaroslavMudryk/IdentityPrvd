using Extensions.DeviceDetector;
using FluentValidation;
using IdentityPrvd.Features.Authentication.Signin.Dtos;
using IdentityPrvd.Features.Authentication.Signin.Dtos.Validators;
using IdentityPrvd.Features.Authentication.Signin.Services;
using IdentityPrvd.Services.Security;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Features.Authentication.Signin;

public static class SigninDependencies
{
    public static IServiceCollection AddSigninDependencies(this IServiceCollection services)
    {
        services.AddScoped<IValidator<PasswordSigninDto>, PasswordSigninDtoValidator>();
        services.AddScoped<IValidator<PasswordlessSigninDto>, PasswordlessSigninDtoValidator>();
        services.AddScoped<PasswordSigninService>();
        services.AddScoped<MfaSigninService>();
        services.AddScoped<PasswordlessSigninService>();
        services.AddScoped<SigninOrchestrator>();
        services.AddScoped<SigninOptionsOrchestrator>();
        services.AddScoped<IUserSecureService, UserSecureService>();
        services.AddDeviceDetector();

        return services;
    }
}
