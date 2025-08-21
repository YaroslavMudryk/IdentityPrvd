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
        return services;
    }

    public static IServiceCollection AddIdentityPrvd(this IServiceCollection services, Action<IdentityPrvdBuilder> builder)
    {
        var identityBuilder = new IdentityPrvdBuilder(services);
        builder.Invoke(identityBuilder);
        return services;
    }

    public static IServiceCollection AddIdentityPrvd(this IServiceCollection services, IConfiguration configuration)
    {
        var identityOptions = configuration.GetSection("IdentityPrvd").Get<IdentityPrvdOptions>();
        var builder = new IdentityPrvdBuilder(services)
        {
            Options = identityOptions
        };
        return services;
    }

    public static IServiceCollection AddIdentityPrvd(this IServiceCollection services, IConfiguration configuration, Action<IdentityPrvdBuilder> builder)
    {
        var identityOptions = configuration.GetSection("IdentityPrvd").Get<IdentityPrvdOptions>();
        var identityBuilder = new IdentityPrvdBuilder(services, identityOptions);
        builder.Invoke(identityBuilder);
        return services;
    }
}
