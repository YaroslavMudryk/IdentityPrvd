using FluentValidation;
using IdentityPrvd.Features.Authorization.Claims.Dtos;
using IdentityPrvd.Features.Authorization.Claims.Dtos.Validators;
using IdentityPrvd.Features.Authorization.Claims.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Features.Authorization.Claims;

public static class ClaimsDependencies
{
    public static IServiceCollection AddClaimsDependencies(this IServiceCollection services)
    {
        services.AddScoped<GetClaimsOrchestrator>();
        services.AddScoped<CreateClaimOrchestrator>();
        services.AddScoped<UpdateClaimOrchestrator>();
        services.AddScoped<DeleteClaimOrchestrator>();

        services.AddScoped<IValidator<CreateClaimDto>, CreateClaimDtoValidator>();
        services.AddScoped<IValidator<UpdateClaimDto>, UpdateClaimDtoValidator>();

        return services;
    }
}
