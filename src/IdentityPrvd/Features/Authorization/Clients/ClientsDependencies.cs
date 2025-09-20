using IdentityPrvd.Features.Authorization.Clients.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Features.Authorization.Clients;

public static class ClientsDependencies
{
    public static IServiceCollection AddClientsDependencies(this IServiceCollection services)
    {
        services.AddScoped<CreateClientOrchestrator>();
        services.AddScoped<DeleteClientOrchestrator>();
        services.AddScoped<UpdateClientOrchestrator>();
        services.AddScoped<GetClientsOrchestrator>();
        services.AddScoped<UpdateClientClaimsOrchestrator>();
        services.AddScoped<GetClientOrchestrator>();

        return services;
    }
}
