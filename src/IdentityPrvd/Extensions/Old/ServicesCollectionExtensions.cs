using IdentityPrvd.Endpoints;
using IdentityPrvd.Infrastructure.Middleware;
using IdentityPrvd.Options;
using IdentityPrvd.Services.AuthSchemes;
using IdentityPrvd.Services.ServerSideSessions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Redis.OM;

namespace IdentityPrvd.Extensions.Old;

public static class ServicesCollectionExtensions
{
    public static IServiceCollection AddIdentityPrvd(this IServiceCollection services)
    {
        var identityOptions = new IdentityPrvdOptions();
        return services.AddIdentityPrvd(identityOptions);
    }

    public static IServiceCollection AddIdentityPrvd(this IServiceCollection services, Action<IdentityPrvdOptions> actionOptions)
    {
        var identityOptions = new IdentityPrvdOptions();
        actionOptions.Invoke(identityOptions);
        return services.AddIdentityPrvd(identityOptions);
    }

    public static IServiceCollection AddIdentityPrvd(this IServiceCollection services, IConfiguration configuration)
    {
        var identityOptions = configuration.GetSection("IdentityPrvd").Get<IdentityPrvdOptions>();
        return services.AddIdentityPrvd(identityOptions);
    }

    public static IServiceCollection AddIdentityPrvd(this IServiceCollection services, IConfiguration configuration, Action<IdentityPrvdOptions> actionOptions)
    {
        var identityOptions = configuration.GetSection("IdentityPrvd").Get<IdentityPrvdOptions>();
        actionOptions.Invoke(identityOptions);
        return services.AddIdentityPrvd(identityOptions);
    }

    public static IServiceCollection AddIdentityPrvd(this IServiceCollection services, IdentityPrvdOptions identityOptions)
    {
        // Configure default external providers if not already configured
        identityOptions.ConfigureDefaultProviders();
        
        identityOptions.ValidateAndThrowIfNeeded();
        services.AddScoped(_ => identityOptions);

        // Register core services
        services.AddEndpoints();
        services.AddFeatures();
        services.AddMiddlewares();
        services.AddContexts();
        services.AddProtectionServices();
        services.AddOthers();

        // Register configured services
        identityOptions.Notifiers.RegisterServices(services);
        identityOptions.Sessions.RegisterServices(services);
        identityOptions.Database.RegisterServices(services, identityOptions.Connections);
        identityOptions.Location.RegisterServices(services);

        // Register external providers
        RegisterExternalProviders(services, identityOptions);

        // Register authentication
        RegisterAuthentication(services, identityOptions);

        return services;
    }

    private static void RegisterExternalProviders(IServiceCollection services, IdentityPrvdOptions identityOptions)
    {
        // Register external provider configurators
        services.AddScoped<GoogleProviderConfigurator>();
        services.AddScoped<MicrosoftProviderConfigurator>();
        services.AddScoped<GitHubProviderConfigurator>();
        services.AddScoped<FacebookProviderConfigurator>();
        services.AddScoped<TwitterProviderConfigurator>();
        services.AddScoped<SteamProviderConfigurator>();

        // Register external provider manager
        services.AddScoped<ExternalProviderManager>();
    }

    private static void RegisterAuthentication(IServiceCollection services, IdentityPrvdOptions identityOptions)
    {
        // Build the service provider to use in extension methods
        var serviceProvider = services.BuildServiceProvider();

        // Configure external providers using the provider manager
        var providerManager = serviceProvider.GetRequiredService<ExternalProviderManager>();
        var authBuilder = new IdentityAuthenticationBuilder(services, identityOptions);
        providerManager.ConfigureProviders(authBuilder, identityOptions);

        // Add JWT Bearer authentication
        authBuilder.AddJwtBearer();

        services.AddAuthorization();
    }
}

public static class WebApplicationExtensions
{
    public static WebApplication UseIdentityPrvd(this WebApplication app)
    {
        app.UseMiddleware<CorrelationContextMiddleware>();
        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseServerSideSessions();

        app.MapEndpoints();
        return app;
    }
}
