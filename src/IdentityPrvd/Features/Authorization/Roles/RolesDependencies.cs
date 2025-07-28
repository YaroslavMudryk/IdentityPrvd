using FluentValidation;
using IdentityPrvd.Features.Authorization.Roles.Dtos;
using IdentityPrvd.Features.Authorization.Roles.Dtos.Validators;
using IdentityPrvd.Features.Authorization.Roles.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Features.Authorization.Roles;

public static class RolesDependencies
{
    public static IServiceCollection AddRolesDependencies(this IServiceCollection services)
    {
        services.AddScoped<GetRolesOrchestrator>();
        services.AddScoped<CreateRoleOrchestrator>();
        services.AddScoped<UpdateRoleOrchestrator>();
        services.AddScoped<DeleteRoleOrchestrator>();
        services.AddScoped<IValidator<CreateRoleDto>, CreateRoleDtoValidator>();
        services.AddScoped<IValidator<UpdateRoleDto>, UpdateRoleDtoValidator>();
        services.AddScoped<DefaultRoleService>();
        return services;
    }
}
