using Extensions.DeviceDetector;
using FluentValidation;
using IdentityPrvd.WebApi.Api;
using IdentityPrvd.WebApi.Features.Signin.DataAccess;
using IdentityPrvd.WebApi.Features.Signin.Dtos;
using IdentityPrvd.WebApi.Features.Signin.Dtos.Validators;
using IdentityPrvd.WebApi.Features.Signin.Services;
using Microsoft.AspNetCore.Authorization;

namespace IdentityPrvd.WebApi.Features.Signin;

public class SigninEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/signin",
            [AllowAnonymous] async (SigninRequestDto dto, SigninOrchestrator orc) =>
            {
                var result = await orc.SigninAsync(dto);
                return Results.Ok(result.MapToResponse());
            }).WithTags("Signin");
    }
}

public static class SigninDependencies
{
    public static IServiceCollection AddSigninDependencies(this IServiceCollection services)
    {
        services.AddScoped<IValidator<SigninRequestDto>, SigninRequestDtoValidator>();
        services.AddScoped<SigninOrchestrator>();
        services.AddHttpClient<ILocationService, IpApiLocationService>("Location", options =>
        {
            options.BaseAddress = new Uri("http://ip-api.com");
        });
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserSecureService, UserSecureService>();

        services.AddScoped<IClientsQuery, ClientsQuery>();
        services.AddScoped<SessionRepo>();
        services.AddScoped<IUsersQuery, UsersQuery>();
        services.AddDeviceDetector();

        return services;
    }
}
