using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IdentityPrvd.Endpoints;

public static class EndpointsExtensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services)
    {
        var endpointType = typeof(IEndpoint);
        var endpointsDescriptors = endpointType.Assembly
            .DefinedTypes
            .Where(type => type.ImplementedInterfaces.Any(s => s == endpointType))
            .Select(type => ServiceDescriptor.Transient(endpointType, type))
            .ToArray();

        services.TryAddEnumerable(endpointsDescriptors);

        return services;
    }

    public static IApplicationBuilder MapEndpoints(this WebApplication app)
    {
        var endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();
        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(app);
        }
        return app;
    }
}
