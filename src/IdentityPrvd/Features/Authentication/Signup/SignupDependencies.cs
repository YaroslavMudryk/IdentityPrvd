using FluentValidation;
using IdentityPrvd.Features.Authentication.Signup.Dtos;
using IdentityPrvd.Features.Authentication.Signup.Dtos.Validators;
using IdentityPrvd.Features.Authentication.Signup.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Features.Authentication.Signup;

public static class SignupDependencies
{
    public static IServiceCollection AddSignupDependencies(this IServiceCollection services)
    {
        services.AddScoped<SignupOrchestrator>();
        services.AddScoped<IValidator<SignupRequestDto>, SignupRequestDtoValidator>();
        return services;
    }
}
