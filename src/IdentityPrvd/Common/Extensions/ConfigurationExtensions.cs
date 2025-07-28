using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IdentityPrvd.Common.Extensions;

public static class ConfigurationExtensions
{
    public static IServiceCollection AddScopedConfiguration<T>(this IServiceCollection services, IConfiguration configuration, string section = null!)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.
            Configure<T>(configuration.GetSection(section ?? typeof(T).Name))
            .AddScoped(provider => provider.GetService<IOptionsSnapshot<T>>()!.Value);
        return services;
    }


    public static IServiceCollection AddSingletonConfiguration<T>(this IServiceCollection services, IConfiguration configuration, string section = null!)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .Configure<T>(configuration.GetSection(section ?? typeof(T).Name))
            .AddSingleton(provider => provider.GetService<IOptions<T>>()!.Value);
        return services;
    }
}
