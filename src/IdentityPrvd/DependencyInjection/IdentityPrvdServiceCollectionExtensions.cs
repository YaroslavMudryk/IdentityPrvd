using IdentityPrvd.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.DependencyInjection;

public static class IdentityPrvdServiceCollectionExtensions
{
    public static IIdentityPrvdBuilder AddIdentityPrvdBuilder(this IServiceCollection services) => new IdentityPrvdBuilder(services);

    public static IIdentityPrvdBuilder AddIdentityPrvd(this IServiceCollection services)
    {
        var identityOptions = new IdentityPrvdOptions();        
        return services.AddIdentityPrvd(identityOptions);
    }

    public static IIdentityPrvdBuilder AddIdentityPrvd(this IServiceCollection services, Action<IdentityPrvdOptions> setupAction)
    {
        var identityOptions = new IdentityPrvdOptions();
        setupAction.Invoke(identityOptions);
        return services.AddIdentityPrvd(identityOptions);
    }

    public static IIdentityPrvdBuilder AddIdentityPrvd(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<IdentityPrvdOptions>(configuration);
        return services.AddIdentityPrvd();
    }

    public static IIdentityPrvdBuilder AddIdentityPrvd(this IServiceCollection services, IConfiguration configuration, Action<IdentityPrvdOptions> setupAction)
    {
        var identityOptions = configuration.GetSection("IdentityPrvd").Get<IdentityPrvdOptions>();
        setupAction.Invoke(identityOptions);
        return services.AddIdentityPrvd(identityOptions);
    }

    public static IIdentityPrvdBuilder AddIdentityPrvd(this IServiceCollection services, IdentityPrvdOptions identityOptions)
    {
        identityOptions.ValidateAndThrowIfNeeded();
        services.AddScoped(_ => identityOptions);

        var builder = services.AddIdentityPrvdBuilder();
        builder.Option = identityOptions;

        builder
            .AddCoreServices()
            .AddRequiredServices()
            .AddFakeEmailNotifier()
            .AddFakeSmsNotifier()
            .AddIpApiLocationService()
            .AddMiddlewares()
            .AddContexts()
            .AddEndpoints()
            .AddProtectionServices()
            .AddRedisSessionServices()
            .AddEfTransaction()
            .AddEfStores()
            .AddEfQueries()
            .AddDefaultContext();

        return builder;
    }
}
