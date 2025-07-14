using Microsoft.Extensions.Options;

namespace IdentityPrvd.WebApi.Extensions;

public static class ConfigurationExtensions
{
    public static IHostApplicationBuilder AddScopedConfiguration<T>(this IHostApplicationBuilder builder, string section = null!)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services
            .Configure<T>(builder.Configuration.GetSection(section ?? typeof(T).Name))
            .AddScoped(provider => provider.GetService<IOptionsSnapshot<T>>()!.Value);

        return builder;
    }

    public static IServiceCollection AddScopedConfiguration<T>(this IServiceCollection services, IConfiguration configuration, string section = null!)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return services.
            Configure<T>(configuration.GetSection(section ?? typeof(T).Name))
            .AddScoped(provider => provider.GetService<IOptionsSnapshot<T>>()!.Value);
    }


    public static IHostApplicationBuilder AddSingletonConfiguration<T>(this IHostApplicationBuilder builder, string section = null!)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services
            .Configure<T>(builder.Configuration.GetSection(section ?? typeof(T).Name))
            .AddSingleton(provider => provider.GetService<IOptions<T>>()!.Value);

        return builder;
    }
}
