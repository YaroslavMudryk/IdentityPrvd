using FluentValidation;
using IdentityPrvd.WebApi.Features.Roles.DataAccess;
using IdentityPrvd.WebApi.Features.Roles.Dtos;
using IdentityPrvd.WebApi.Features.Roles.Dtos.Validators;
using IdentityPrvd.WebApi.Features.Roles.Services;

namespace IdentityPrvd.WebApi.Features.Roles;

public static class RolesDependencies
{
    public static IServiceCollection AddRolesDependencies(this IServiceCollection services)
    {
        services.AddScoped<GetRolesOrchestrator>();
        services.AddScoped<CreateRoleOrchestrator>();
        services.AddScoped<UpdateRoleOrchestrator>();
        services.AddScoped<DeleteRoleOrchestrator>();
        services.AddScoped<RolesQuery>();
        services.AddScoped<RoleRepo>();
        services.AddScoped<RoleClaimRepo>();

        services.AddScoped<IValidator<CreateRoleDto>, CreateRoleDtoValidator>();
        services.AddScoped<IValidator<UpdateRoleDto>, UpdateRoleDtoValidator>();
        services.AddScoped<IRolesValidatorQuery, RolesValidatorQuery>();
        services.AddScoped<DefaultRoleService>();

        return services;
    }
}
