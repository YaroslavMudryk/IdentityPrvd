using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Tests.IntegrationInfra;

internal static class HelperExtensions
{
    public static IServiceCollection Replace<T>(this IServiceCollection services, Action<IServiceCollection> with)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(T));
        if (descriptor != null) services.Remove(descriptor);
        with(services);
        return services;
    }

    public static IServiceCollection Remove<T>(this IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(T));
        if (descriptor != null) services.Remove(descriptor);
        return services;
    }
}
