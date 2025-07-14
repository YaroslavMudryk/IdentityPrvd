using IdentityPrvd.WebApi.Api;
using IdentityPrvd.WebApi.Features.RestorePassword.DataAccess;
using IdentityPrvd.WebApi.Features.RestorePassword.Dtos;
using IdentityPrvd.WebApi.Features.RestorePassword.Services;

namespace IdentityPrvd.WebApi.Features.RestorePassword;

public class StartRestorePasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/start-restore-password",
            async (StartRestorePasswordDto dto, StartRestorePasswordOrchestrator orc) =>
            {
                var startedRestoreDto = await orc.StartRestorePasswordAsync(dto);
                return Results.Ok(startedRestoreDto.MapToResponse());
            });
    }
}

public class RestorePasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/restore-password",
            async (RestorePasswordDto dto, RestorePasswordOrchestrator orc) =>
            {
                await orc.RestorePasswordAsync(dto);
                return Results.NoContent();
            });
    }
}

public static class RestorePasswordDependencies
{
    public static IServiceCollection AddRestorePasswordDependencies(this IServiceCollection services)
    {
        services.AddScoped<StartRestorePasswordOrchestrator>();
        services.AddScoped<RestorePasswordOrchestrator>();
        services.AddScoped<ConfirmRepo>();
        services.AddScoped<PasswordRepo>();
        services.AddScoped<UserRepo>();
        return services;
    }
}
