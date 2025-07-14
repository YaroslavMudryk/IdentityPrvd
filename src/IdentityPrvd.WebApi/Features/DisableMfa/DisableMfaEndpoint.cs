using IdentityPrvd.WebApi.Features.DisableMfa.Services;

namespace IdentityPrvd.WebApi.Features.DisableMfa;

public class DisableMfaEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/identity/mfa",
            async (string code, DisableMfaOrchestrator orc) =>
            {
                await orc.DisableMfaAsync(code);
                return Results.NoContent();
            }).WithTags("Mfa");
    }
}

public static class DisableMfaDependencies
{
    public static IServiceCollection AddDisableMfaDependencies(this IServiceCollection services)
    {
        services.AddScoped<DisableMfaOrchestrator>();
        return services;
    }
}
