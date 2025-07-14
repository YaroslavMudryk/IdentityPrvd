using FluentValidation;
using IdentityPrvd.WebApi.Api;
using IdentityPrvd.WebApi.Features.Signup.DataAccess;
using IdentityPrvd.WebApi.Features.Signup.Dtos;
using IdentityPrvd.WebApi.Features.Signup.Dtos.Validators;
using IdentityPrvd.WebApi.Features.Signup.Services;
using Microsoft.AspNetCore.Authorization;

namespace IdentityPrvd.WebApi.Features.Signup;

public class SignupEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/signup",
            [AllowAnonymous] async (SignupRequestDto dto, SignupOrchestrator orc) =>
            {
                var result = await orc.SignupAsync(dto);
                return Results.Json(result.MapToResponse(), statusCode: 201);
            }).WithTags("Signup");
    }
}

public static class SignupDependencies
{
    public static IServiceCollection AddSignupDependencies(this IServiceCollection services)
    {
        services.AddScoped<UserRepo>();
        services.AddScoped<ConfirmRepo>();
        services.AddScoped<UserRoleRepo>();
        services.AddScoped<PasswordRepo>();
        services.AddScoped<RolesQuery>();
        services.AddScoped<SignupOrchestrator>();
        services.AddScoped<IUserValidatorQuery, UserValidatorQuery>();
        services.AddScoped<IValidator<SignupRequestDto>, SignupRequestDtoValidator>();
        return services;
    }
}
