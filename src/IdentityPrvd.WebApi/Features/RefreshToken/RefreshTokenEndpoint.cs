using FluentValidation;
using IdentityPrvd.WebApi.Api;
using IdentityPrvd.WebApi.Features.RefreshToken.DataAccess;
using IdentityPrvd.WebApi.Features.RefreshToken.Dtos;
using IdentityPrvd.WebApi.Features.RefreshToken.Dtos.Validators;
using IdentityPrvd.WebApi.Features.RefreshToken.Services;
using Microsoft.AspNetCore.Authorization;

namespace IdentityPrvd.WebApi.Features.RefreshToken;

public class RefreshTokenEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/refresh-token",
            [AllowAnonymous] async (RefreshTokenDto dto, RefreshTokenOrchestrator orc) =>
            {
                var result = await orc.SigninByRefreshTokenAsync(dto);
                return Results.Ok(result.MapToResponse());
            }).WithTags("Signin");
    }
}

public static class RefreshTokenDependencies
{
    public static IServiceCollection AddRefreshTokenDependencies(this IServiceCollection services)
    {
        services.AddScoped<RefreshTokenOrchestrator>();
        services.AddScoped<IValidator<RefreshTokenDto>, RefreshTokenDtoValidator>();
        services.AddScoped<RefreshTokenRepo>();
        services.AddScoped<IRefreshTokensQuery, RefreshTokensQuery>();
        return services;
    }
}
