using IdentityPrvd.WebApi.Features.ChangeLogin.Dtos;
using IdentityPrvd.WebApi.Features.ChangeLogin.Services;

namespace IdentityPrvd.WebApi.Features.ChangeLogin;

public class ChangeLoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/change-login",
            async (ChangeLoginDto dto, ChangeLoginOrchestrator orc) =>
            {
                await orc.ChangeLoginAsync(dto);
                return Results.NoContent();
            });
    }
}

public static class ChangeLoginDependencies
{
    public static IServiceCollection AddChangeLoginDependencies(this IServiceCollection services)
    {
        services.AddScoped<ChangeLoginOrchestrator>();
        return services;
    }
}
