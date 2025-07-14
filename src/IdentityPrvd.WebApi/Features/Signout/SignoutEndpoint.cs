using IdentityPrvd.WebApi.Features.Signout.DataAccess;
using IdentityPrvd.WebApi.Features.Signout.Services;

namespace IdentityPrvd.WebApi.Features.Signout;

public class SignoutEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/signout",
            async (SignoutOrchestrator orc, bool everywhere = false) =>
            {
                await orc.SignoutAsync(everywhere);
                return Results.NoContent();
            }).WithTags("Signout");
    }
}

public static class SignoutDependencies
{
    public static IServiceCollection AddSignoutDependencies(this IServiceCollection services)
    {
        services.AddScoped<SignoutOrchestrator>();
        services.AddScoped<RefreshTokenRepo>();
        services.AddScoped<SessionRepo>();
        return services;
    }
}
