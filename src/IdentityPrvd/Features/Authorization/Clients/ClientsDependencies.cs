using FluentValidation;
using IdentityPrvd.Features.Authorization.Clients.Dtos;
using IdentityPrvd.Features.Authorization.Clients.Dtos.Validators;
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
        services.AddScoped<UpdateClientPermissionsOrchestrator>();
        services.AddScoped<GetClientOrchestrator>();

        services.AddScoped<IValidator<CreateClientDto>, CreateClientDtoValidator>();
        services.AddScoped<IValidator<UpdateClientDto>, UpdateClientDtoValidator>();
        services.AddScoped<IValidator<UpdateClientPermissionsDto>, UpdateClientPermissionsDtoValidator>();

        return services;
    }
}
