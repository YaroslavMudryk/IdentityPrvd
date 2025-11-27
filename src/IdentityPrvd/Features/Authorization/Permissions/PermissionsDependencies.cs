using FluentValidation;
using IdentityPrvd.Features.Authorization.Permissions.Dtos;
using IdentityPrvd.Features.Authorization.Permissions.Dtos.Validators;
using IdentityPrvd.Features.Authorization.Permissions.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Features.Authorization.Permissions;

public static class PermissionsDependencies
{
    public static IServiceCollection AddPermissionsDependencies(this IServiceCollection services)
    {
        services.AddScoped<GetPermissionsOrchestrator>();
        services.AddScoped<CreatePermissionOrchestrator>();
        services.AddScoped<UpdatePermissionOrchestrator>();
        services.AddScoped<DeletePermissionOrchestrator>();

        services.AddScoped<IValidator<CreatePermissionDto>, CreatePermissionDtoValidator>();
        services.AddScoped<IValidator<UpdatePermissionDto>, UpdatePermissionDtoValidator>();

        return services;
    }
}
