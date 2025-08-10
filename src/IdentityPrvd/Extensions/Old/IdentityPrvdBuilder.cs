using IdentityPrvd.Common.Helpers;
using IdentityPrvd.Contexts;
using IdentityPrvd.Endpoints;
using IdentityPrvd.Features.Authentication.ChangeLogin;
using IdentityPrvd.Features.Authentication.ChangePassword;
using IdentityPrvd.Features.Authentication.ExternalSignin;
using IdentityPrvd.Features.Authentication.LinkExternalSignin;
using IdentityPrvd.Features.Authentication.RestorePassword;
using IdentityPrvd.Features.Authentication.Signin;
using IdentityPrvd.Features.Authentication.SigninOptions;
using IdentityPrvd.Features.Authentication.Signout;
using IdentityPrvd.Features.Authentication.Signup;
using IdentityPrvd.Features.Authorization.Claims;
using IdentityPrvd.Features.Authorization.Roles;
using IdentityPrvd.Features.Personal.Contacts;
using IdentityPrvd.Features.Personal.Devices;
using IdentityPrvd.Features.Security.Mfa.DisableMfa;
using IdentityPrvd.Features.Security.Mfa.EnableMfa;
using IdentityPrvd.Features.Security.RefreshToken;
using IdentityPrvd.Features.Security.Sessions.GetSessions;
using IdentityPrvd.Features.Security.Sessions.RevokeSessions;
using IdentityPrvd.Infrastructure.Middleware;
using IdentityPrvd.Options;
using IdentityPrvd.Services.AuthSchemes;
using IdentityPrvd.Services.Security;
using IdentityPrvd.Services.ServerSideSessions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IdentityPrvd.Extensions.Old;

/// <summary>
/// Main builder for configuring IdentityPrvd services
/// </summary>
public class IdentityPrvdBuilder(IServiceCollection services, IdentityPrvdOptions options)
{

    /// <summary>
    /// Configure database services
    /// </summary>
    public DatabaseBuilder WithDatabase()
    {
        return new DatabaseBuilder(services, options);
    }

    /// <summary>
    /// Configure notification services
    /// </summary>
    public NotifierBuilder WithNotifier()
    {
        return new NotifierBuilder(services);
    }

    /// <summary>
    /// Configure session services
    /// </summary>
    public SessionBuilder WithSessions()
    {
        return new SessionBuilder(services, options);
    }

    /// <summary>
    /// Configure external authentication providers
    /// </summary>
    public ExternalProviderBuilder WithExternalProviders()
    {
        return new ExternalProviderBuilder(services);
    }

    /// <summary>
    /// Configure authentication and authorization
    /// </summary>
    public IdentityAuthenticationBuilder WithAuthentication()
    {
        return new IdentityAuthenticationBuilder(services, options);
    }

    /// <summary>
    /// Configure all core services (endpoints, features, middleware, etc.)
    /// </summary>
    public IdentityPrvdBuilder WithCoreServices()
    {
        services.AddEndpoints();
        services.AddFeatures();
        services.AddMiddlewares();
        services.AddContexts();
        services.AddProtectionServices();
        services.AddOthers();
        
        return this;
    }

    /// <summary>
    /// Build and register all services
    /// </summary>
    public IServiceCollection Build()
    {
        // Configure default external providers if not already configured
        options.ConfigureDefaultProviders();
        
        options.ValidateAndThrowIfNeeded();
        services.AddScoped(_ => options);

        return services;
    }

    private void AddFeatures()
    {
        services.AddSignupDependencies();
        services.AddSigninDependencies();
        services.AddSigninOptionsDependencies();
        services.AddRefreshTokenDependencies();
        services.AddSignoutDependencies();
        services.AddGetSessionsDependencies();
        services.AddRevokeSessionsDependencies();
        services.AddEnableMfaDependencies();
        services.AddDisableMfaDependencies();
        services.AddRolesDependencies();
        services.AddClaimsDependencies();
        services.AddExternalSigninDependencies();
        services.AddLinkExternalProviderDependencies();
        services.AddChangeLoginDependencies();
        services.AddChangePasswordDependencies();
        services.AddRestorePasswordDependencies();
        services.AddContactsDependencies();
        services.AddDevicesDependencies();
    }
}
