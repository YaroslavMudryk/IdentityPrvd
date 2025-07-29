using FluentValidation;
using IdentityPrvd.Features.Authentication.ExternalSignin.Dtos.Validators;
using IdentityPrvd.Features.Authentication.ExternalSignin.Services;
using Microsoft.Extensions.DependencyInjection;
using IdentityPrvd.Features.Authentication.ExternalSignin.Dtos;

namespace IdentityPrvd.Features.Authentication.ExternalSignin;

public static class ExternalSigninDependencies
{
    public static void AddExternalSigninDependencies(this IServiceCollection services)
    {
        services.AddScoped<ExternalSigninOrchestrator>();
        services.AddScoped<ExternalUserExstractorService>();

        services.AddScoped<IValidator<ExternalSigninDto>, ExternalSigninDtoValidator>();
    }
}
