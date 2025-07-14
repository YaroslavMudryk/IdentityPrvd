using FluentValidation;
using IdentityPrvd.WebApi.Features.Claims.DataAccess;
using IdentityPrvd.WebApi.Features.Claims.Dtos;
using IdentityPrvd.WebApi.Features.Claims.Dtos.Validators;
using IdentityPrvd.WebApi.Features.Claims.Services;

namespace IdentityPrvd.WebApi.Features.Claims;

public static class ClaimsDependencies
{
    public static IServiceCollection AddClaimsDependencies(this IServiceCollection services)
    {
        services.AddScoped<GetClaimsOrchestrator>();
        services.AddScoped<CreateClaimOrchestrator>();
        services.AddScoped<UpdateClaimOrchestrator>();
        services.AddScoped<DeleteClaimOrchestrator>();
        services.AddScoped<ClaimsQuery>();
        services.AddScoped<ClaimRepo>();
        services.AddScoped<IClaimsValidatorQuery, ClaimsValidatorQuery>();

        services.AddScoped<IValidator<CreateClaimDto>, CreateClaimDtoValidator>();
        services.AddScoped<IValidator<UpdateClaimDto>, UpdateClaimDtoValidator>();

        return services;
    }
}
