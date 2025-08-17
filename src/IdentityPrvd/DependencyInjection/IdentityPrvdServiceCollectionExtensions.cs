using IdentityPrvd.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.DependencyInjection;

public static class IdentityPrvdServiceCollectionExtensions
{
    public static IIdentityPrvdBuilder AddIdentityPrvdBuilder(this IServiceCollection services) => new IdentityPrvdBuilder(services);

    public static IServiceCollection AddIdentityPrvd(this IServiceCollection services)
    {
        var builder = new IdentityPrvdBuilder(services);
        return services.AddIdentityPrvd(builder);
    }

    public static IServiceCollection AddIdentityPrvd(this IServiceCollection services, Action<IdentityPrvdBuilder> builder)
    {
        var identityBuilder = new IdentityPrvdBuilder(services);
        builder.Invoke(identityBuilder);
        return services.AddIdentityPrvd(identityBuilder);
    }

    public static IServiceCollection AddIdentityPrvd(this IServiceCollection services, IConfiguration configuration)
    {
        var identityOptions = configuration.GetSection("IdentityPrvd").Get<IdentityPrvdOptions>();
        var builder = new IdentityPrvdBuilder(services)
        {
            Option = identityOptions
        };
        return services.AddIdentityPrvd(builder);
    }

    public static IServiceCollection AddIdentityPrvd(this IServiceCollection services, IConfiguration configuration, Action<IdentityPrvdBuilder> builder)
    {
        var identityOptions = configuration.GetSection("IdentityPrvd").Get<IdentityPrvdOptions>();
        var identityBuilder = new IdentityPrvdBuilder(services);
        builder.Invoke(identityBuilder);
        return services.AddIdentityPrvd(identityBuilder);
    }

    private static IServiceCollection AddIdentityPrvd(this IServiceCollection services, IdentityPrvdBuilder builder)
    {
        services.AddScoped(_ => builder.Option);

        builder
            .AddCoreServices()
            .AddRequiredServices()
            .AddAuthentication()
            .AddFakeEmailNotifier()
            .AddFakeSmsNotifier()
            .AddFakeLocationService()
            .AddMiddlewares()
            .AddContexts()
            .AddEndpoints()
            .AddProtectionServices()
            .AddRedisSessionManagerStore()
            .AddSessionServices()
            .AddEfTransaction()
            .AddEfStores()
            .AddEfQueries()
            .AddDefaultDbContext();

        return services;
    }
}
