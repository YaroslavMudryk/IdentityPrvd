using IdentityPrvd.WebApi.Features.ChangePassword.DataAccess;
using IdentityPrvd.WebApi.Features.ChangePassword.Dtos;
using IdentityPrvd.WebApi.Features.ChangePassword.Services;

namespace IdentityPrvd.WebApi.Features.ChangePassword;

public class ChangePasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/change-password",
             async (ChangePasswordDto dto, ChangePasswordOrchestrator orc) =>
             {
                 await orc.ChangePasswordAsync(dto);
                 return Results.NoContent();
             });
    }
}

public static class ChangePasswordDependencies
{
    public static IServiceCollection AddChangePasswordDependencies(this IServiceCollection services)
    {
        services.AddScoped<ChangePasswordOrchestrator>();

        services.AddScoped<UserRepo>();
        services.AddScoped<PasswordRepo>();
        services.AddScoped<SessionRepo>();
        services.AddScoped<RefreshTokenRepo>();
        return services;
    }
}
