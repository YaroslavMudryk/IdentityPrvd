using IdentityPrvd.WebApi.Api;
using IdentityPrvd.WebApi.Features.EnableMfa.DataAccess;
using IdentityPrvd.WebApi.Features.EnableMfa.Dtos;
using IdentityPrvd.WebApi.Features.EnableMfa.Services;

namespace IdentityPrvd.WebApi.Features.EnableMfa;

public class EnableMfaEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/mfa",
            async (MfaDto dto, EnableMfaOrchestrator orc) =>
            {
                var mfaResult = await orc.EnableMfaAsync(dto);
                return mfaResult is null ?
                    Results.NoContent() :
                    Results.Ok(mfaResult.MapToResponse());
            }).WithTags("Mfa");
    }
}

public static class EnableMfaDependencies
{
    public static IServiceCollection AddEnableMfaDependencies(this IServiceCollection services)
    {
        services.AddScoped<EnableMfaOrchestrator>();
        services.AddScoped<MfaRepo>();
        return services;
    }
}
