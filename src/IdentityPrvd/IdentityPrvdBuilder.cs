using IdentityPrvd.Common.Helpers;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Data.Transactions;
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
using IdentityPrvd.Infrastructure.Caching;
using IdentityPrvd.Infrastructure.Database.Context;
using IdentityPrvd.Infrastructure.Database.Transactions;
using IdentityPrvd.Infrastructure.Middleware;
using IdentityPrvd.Options;
using IdentityPrvd.Services.AuthSchemes;
using IdentityPrvd.Services.Notification;
using IdentityPrvd.Services.Security;
using IdentityPrvd.Services.ServerSideSessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Redis.OM;
using Redis.OM.Contracts;
using System.Text;

namespace IdentityPrvd;

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

    private void AddMiddlewares()
    {
        services.AddTransient<CorrelationContextMiddleware>();
        services.AddTransient<ServerSideSessionMiddleware>();
        services.AddTransient<GlobalExceptionHandlerMiddleware>();
    }

    private void AddContexts()
    {
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<ICurrentContext, CurrentContext>();
    }

    private void AddProtectionServices()
    {
        services.AddScoped<IProtectionService, AesProtectionService>();
        services.AddScoped<IMfaService, TotpMfaService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IHasher, Sha512Hasher>();
    }

    private void AddOthers()
    {
        services.AddScoped<UserHelper>();
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IAuthSchemes, DefaultAuthSchemes>();
    }
}

/// <summary>
/// Builder for database services with support for different implementations
/// </summary>
public class DatabaseBuilder
{
    private readonly IServiceCollection _services;
    private readonly IdentityPrvdOptions _options;

    public DatabaseBuilder(IServiceCollection services, IdentityPrvdOptions options)
    {
        _services = services;
        _options = options;
    }

    /// <summary>
    /// Add all database services with default EF Core implementation
    /// </summary>
    public DatabaseBuilder AddAll()
    {
        AddQueries();
        AddStores();
        AddDbContext();
        AddRedis();
        AddTransactions();
        return this;
    }

    /// <summary>
    /// Add database queries with EF Core implementation
    /// </summary>
    public DatabaseBuilder AddQueries()
    {
        _services.AddScoped<IClaimsQuery, EfClaimsQuery>();
        _services.AddScoped<IClientsQuery, EfClientsQuery>();
        _services.AddScoped<IRefreshTokensQuery, EfRefreshTokensQuery>();
        _services.AddScoped<IRolesQuery, EfRolesQuery>();
        _services.AddScoped<ISessionsQuery, EfSessionsQuery>();
        _services.AddScoped<IUserLoginsQuery, EfUserLoginsQuery>();
        _services.AddScoped<IUsersQuery, EfUsersQuery>();
        _services.AddScoped<IContactsQuery, EfContactsQuery>();
        _services.AddScoped<IDevicesQuery, EfDevicesQuery>();
        return this;
    }

    /// <summary>
    /// Add database stores with EF Core implementation
    /// </summary>
    public DatabaseBuilder AddStores()
    {
        _services.AddScoped<IClaimStore, EfClaimStore>();
        _services.AddScoped<IConfirmStore, EfConfirmStore>();
        _services.AddScoped<IMfaStore, EfMfaStore>();
        _services.AddScoped<IPasswordStore, EfPasswordStore>();
        _services.AddScoped<IRefreshTokenStore, EfRefreshTokenStore>();
        _services.AddScoped<IRoleClaimStore, EfRoleClaimStore>();
        _services.AddScoped<IRoleStore, EfRoleStore>();
        _services.AddScoped<Data.Stores.ISessionStore, EfSessionStore>();
        _services.AddScoped<IUserLoginStore, EfUserLoginStore>();
        _services.AddScoped<IUserRoleStore, EfUserRoleStore>();
        _services.AddScoped<IUserStore, EfUserStore>();
        _services.AddScoped<IContactStore, EfContactStore>();
        _services.AddScoped<IDeviceStore, EfDeviceStore>();
        return this;
    }

    /// <summary>
    /// Add Entity Framework context with PostgreSQL
    /// </summary>
    public DatabaseBuilder AddDbContext()
    {
        _services.AddDbContext<IdentityPrvdContext>(options =>
            options.UseNpgsql(_options.Connections.Db)
                .UseSnakeCaseNamingConvention());
        return this;
    }

    /// <summary>
    /// Add Redis connection
    /// </summary>
    public DatabaseBuilder AddRedis()
    {
        _services.AddScoped<IRedisConnectionProvider>(_ =>
            new RedisConnectionProvider(_options.Connections.Redis));
        return this;
    }

    /// <summary>
    /// Add transaction services with EF Core implementation
    /// </summary>
    public DatabaseBuilder AddTransactions()
    {
        _services.AddScoped<ITransactionManager, EfCoreTransactionManager>();
        return this;
    }

    /// <summary>
    /// Use custom queries implementation
    /// </summary>
    public DatabaseBuilder UseCustomQueries<TClaimsQuery, TClientsQuery, TRefreshTokensQuery, TRolesQuery, TSessionsQuery, TUserLoginsQuery, TUsersQuery, TContactsQuery, TDevicesQuery>()
        where TClaimsQuery : class, IClaimsQuery
        where TClientsQuery : class, IClientsQuery
        where TRefreshTokensQuery : class, IRefreshTokensQuery
        where TRolesQuery : class, IRolesQuery
        where TSessionsQuery : class, ISessionsQuery
        where TUserLoginsQuery : class, IUserLoginsQuery
        where TUsersQuery : class, IUsersQuery
        where TContactsQuery : class, IContactsQuery
        where TDevicesQuery : class, IDevicesQuery
    {
        _services.AddScoped<IClaimsQuery, TClaimsQuery>();
        _services.AddScoped<IClientsQuery, TClientsQuery>();
        _services.AddScoped<IRefreshTokensQuery, TRefreshTokensQuery>();
        _services.AddScoped<IRolesQuery, TRolesQuery>();
        _services.AddScoped<ISessionsQuery, TSessionsQuery>();
        _services.AddScoped<IUserLoginsQuery, TUserLoginsQuery>();
        _services.AddScoped<IUsersQuery, TUsersQuery>();
        _services.AddScoped<IContactsQuery, TContactsQuery>();
        _services.AddScoped<IDevicesQuery, TDevicesQuery>();
        return this;
    }

    /// <summary>
    /// Use custom stores implementation
    /// </summary>
    public DatabaseBuilder UseCustomStores<TClaimStore, TConfirmStore, TMfaStore, TPasswordStore, TRefreshTokenStore, TRoleClaimStore, TRoleStore, TSessionStore, TUserLoginStore, TUserRoleStore, TUserStore, TContactStore, TDeviceStore>()
        where TClaimStore : class, IClaimStore
        where TConfirmStore : class, IConfirmStore
        where TMfaStore : class, IMfaStore
        where TPasswordStore : class, IPasswordStore
        where TRefreshTokenStore : class, IRefreshTokenStore
        where TRoleClaimStore : class, IRoleClaimStore
        where TRoleStore : class, IRoleStore
        where TSessionStore : class, Data.Stores.ISessionStore
        where TUserLoginStore : class, IUserLoginStore
        where TUserRoleStore : class, IUserRoleStore
        where TUserStore : class, IUserStore
        where TContactStore : class, IContactStore
        where TDeviceStore : class, IDeviceStore
    {
        _services.AddScoped<IClaimStore, TClaimStore>();
        _services.AddScoped<IConfirmStore, TConfirmStore>();
        _services.AddScoped<IMfaStore, TMfaStore>();
        _services.AddScoped<IPasswordStore, TPasswordStore>();
        _services.AddScoped<IRefreshTokenStore, TRefreshTokenStore>();
        _services.AddScoped<IRoleClaimStore, TRoleClaimStore>();
        _services.AddScoped<IRoleStore, TRoleStore>();
        _services.AddScoped<Data.Stores.ISessionStore, TSessionStore>();
        _services.AddScoped<IUserLoginStore, TUserLoginStore>();
        _services.AddScoped<IUserRoleStore, TUserRoleStore>();
        _services.AddScoped<IUserStore, TUserStore>();
        _services.AddScoped<IContactStore, TContactStore>();
        _services.AddScoped<IDeviceStore, TDeviceStore>();
        return this;
    }

    /// <summary>
    /// Use custom transaction manager
    /// </summary>
    public DatabaseBuilder UseCustomTransactionManager<TTransactionManager>() where TTransactionManager : class, ITransactionManager
    {
        _services.AddScoped<ITransactionManager, TTransactionManager>();
        return this;
    }

    /// <summary>
    /// Use custom Redis connection provider
    /// </summary>
    public DatabaseBuilder UseCustomRedisConnection<TRedisConnectionProvider>() where TRedisConnectionProvider : class, IRedisConnectionProvider
    {
        _services.AddScoped<IRedisConnectionProvider, TRedisConnectionProvider>();
        return this;
    }

    /// <summary>
    /// Return to main builder
    /// </summary>
    public IdentityPrvdBuilder And()
    {
        return new IdentityPrvdBuilder(_services, _options);
    }
}

/// <summary>
/// Builder for notification services with support for different implementations
/// </summary>
public class NotifierBuilder
{
    private readonly IServiceCollection _services;

    public NotifierBuilder(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Add email service with fake implementation (default)
    /// </summary>
    public NotifierBuilder AddEmail()
    {
        _services.AddScoped<IEmailService, FakeEmailService>();
        return this;
    }

    /// <summary>
    /// Add SMS service with fake implementation (default)
    /// </summary>
    public NotifierBuilder AddSms()
    {
        _services.AddScoped<ISmsService, FakeSmsService>();
        return this;
    }

    /// <summary>
    /// Add all notification services with fake implementations (default)
    /// </summary>
    public NotifierBuilder AddAll()
    {
        return AddEmail().AddSms();
    }

    /// <summary>
    /// Use custom email service implementation
    /// </summary>
    public NotifierBuilder UseCustomEmailService<TEmailService>() where TEmailService : class, IEmailService
    {
        _services.AddScoped<IEmailService, TEmailService>();
        return this;
    }

    /// <summary>
    /// Use custom SMS service implementation
    /// </summary>
    public NotifierBuilder UseCustomSmsService<TSmsService>() where TSmsService : class, ISmsService
    {
        _services.AddScoped<ISmsService, TSmsService>();
        return this;
    }

    /// <summary>
    /// Use Twilio SMS service
    /// </summary>
    public NotifierBuilder UseTwilioSms(string accountSid, string authToken, string fromNumber)
    {
        // This would be implemented when you create a Twilio SMS service
        // _services.AddScoped<ISmsService, TwilioSmsService>();
        // _services.Configure<TwilioOptions>(options =>
        // {
        //     options.AccountSid = accountSid;
        //     options.AuthToken = authToken;
        //     options.FromNumber = fromNumber;
        // });
        return this;
    }

    /// <summary>
    /// Use SendGrid email service
    /// </summary>
    public NotifierBuilder UseSendGridEmail(string apiKey, string fromEmail, string fromName)
    {
        // This would be implemented when you create a SendGrid email service
        // _services.AddScoped<IEmailService, SendGridEmailService>();
        // _services.Configure<SendGridOptions>(options =>
        // {
        //     options.ApiKey = apiKey;
        //     options.FromEmail = fromEmail;
        //     options.FromName = fromName;
        // });
        return this;
    }

    /// <summary>
    /// Return to main builder
    /// </summary>
    public IdentityPrvdBuilder And()
    {
        return new IdentityPrvdBuilder(_services, new IdentityPrvdOptions());
    }
}

/// <summary>
/// Builder for session services with support for different implementations
/// </summary>
public class SessionBuilder
{
    private readonly IServiceCollection _services;
    private readonly IdentityPrvdOptions _options;

    public SessionBuilder(IServiceCollection services, IdentityPrvdOptions options)
    {
        _services = services;
        _options = options;
    }

    /// <summary>
    /// Add server-side sessions with Redis implementation (default)
    /// </summary>
    public SessionBuilder AddServerSideSessions()
    {
        _services.AddScoped<ISessionManager, SessionManager>();
        _services.AddScoped<Infrastructure.Caching.ISessionStore, RedisSessionStore>();
        _services.AddTransient<ServerSideSessionMiddleware>();
        return this;
    }

    /// <summary>
    /// Add all session services with Redis implementation (default)
    /// </summary>
    public SessionBuilder AddAll()
    {
        return AddServerSideSessions();
    }

    /// <summary>
    /// Use custom session manager
    /// </summary>
    public SessionBuilder UseCustomSessionManager<TSessionManager>() where TSessionManager : class, ISessionManager
    {
        _services.AddScoped<ISessionManager, TSessionManager>();
        return this;
    }

    /// <summary>
    /// Use custom session store
    /// </summary>
    public SessionBuilder UseCustomSessionStore<TSessionStore>() where TSessionStore : class, Infrastructure.Caching.ISessionStore
    {
        _services.AddScoped<Infrastructure.Caching.ISessionStore, TSessionStore>();
        return this;
    }

    /// <summary>
    /// Use in-memory session store (for development/testing)
    /// </summary>
    public SessionBuilder UseInMemorySessions()
    {
        _services.AddScoped<Infrastructure.Caching.ISessionStore, InMemorySessionStore>();
        return this;
    }

    /// <summary>
    /// Return to main builder
    /// </summary>
    public IdentityPrvdBuilder And()
    {
        return new IdentityPrvdBuilder(_services, _options);
    }
}

/// <summary>
/// Builder for external authentication providers
/// </summary>
public class ExternalProviderBuilder
{
    private readonly IServiceCollection _services;

    public ExternalProviderBuilder(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Add Google provider
    /// </summary>
    public ExternalProviderBuilder AddGoogle(string clientId, string clientSecret, string? callbackPath = null)
    {
        _services.AddGoogleProvider(clientId, clientSecret, callbackPath);
        return this;
    }

    /// <summary>
    /// Add Microsoft provider
    /// </summary>
    public ExternalProviderBuilder AddMicrosoft(string clientId, string clientSecret, string? callbackPath = null)
    {
        _services.AddMicrosoftProvider(clientId, clientSecret, callbackPath);
        return this;
    }

    /// <summary>
    /// Add GitHub provider
    /// </summary>
    public ExternalProviderBuilder AddGitHub(string clientId, string clientSecret, string? callbackPath = null, IEnumerable<string>? scopes = null)
    {
        _services.AddGitHubProvider(clientId, clientSecret, callbackPath, scopes);
        return this;
    }

    /// <summary>
    /// Add Facebook provider
    /// </summary>
    public ExternalProviderBuilder AddFacebook(string clientId, string clientSecret, string? callbackPath = null)
    {
        _services.AddFacebookProvider(clientId, clientSecret, callbackPath);
        return this;
    }

    /// <summary>
    /// Add Twitter provider
    /// </summary>
    public ExternalProviderBuilder AddTwitter(string clientId, string clientSecret, string? callbackPath = null, IEnumerable<string>? scopes = null)
    {
        _services.AddTwitterProvider(clientId, clientSecret, callbackPath, scopes);
        return this;
    }

    /// <summary>
    /// Add Steam provider
    /// </summary>
    public ExternalProviderBuilder AddSteam(string applicationKey, string? callbackPath = null)
    {
        _services.AddSteamProvider(applicationKey, callbackPath);
        return this;
    }

    /// <summary>
    /// Add custom provider
    /// </summary>
    public ExternalProviderBuilder AddCustom<TConfigurator>(ExternalProviderOptions options) where TConfigurator : class, IExternalProviderConfigurator
    {
        _services.AddExternalProvider<TConfigurator>(options);
        return this;
    }

    /// <summary>
    /// Return to main builder
    /// </summary>
    public IdentityPrvdBuilder And()
    {
        return new IdentityPrvdBuilder(_services, new IdentityPrvdOptions());
    }
}

/// <summary>
/// Builder for authentication and authorization
/// </summary>
public class IdentityAuthenticationBuilder
{
    private readonly IServiceCollection _services;
    private readonly IdentityPrvdOptions _options;
    private readonly Microsoft.AspNetCore.Authentication.AuthenticationBuilder _authBuilder;

    public IdentityAuthenticationBuilder(IServiceCollection services, IdentityPrvdOptions options)
    {
        _services = services;
        _options = options;
        _authBuilder = services.AddAuthentication();
    }

    public Microsoft.AspNetCore.Authentication.AuthenticationBuilder AuthenticationBuilder => _authBuilder;

    /// <summary>
    /// Add JWT Bearer authentication
    /// </summary>
    public IdentityAuthenticationBuilder AddJwtBearer()
    {
        _authBuilder
            .AddCookie("cookie")
            .AddJwtBearer(jwt =>
            {
                jwt.RequireHttpsMetadata = false;
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ClockSkew = TimeSpan.Zero,
                    ValidateIssuer = true,
                    ValidIssuer = _options.Token.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _options.Token.Audience,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_options.Token.SecretKey!)),
                    ValidateIssuerSigningKey = true,
                };
                jwt.SaveToken = true;
            });

        return this;
    }

    /// <summary>
    /// Add authorization
    /// </summary>
    public IdentityAuthenticationBuilder AddAuthorization()
    {
        _services.AddAuthorization();
        return this;
    }

    /// <summary>
    /// Add all authentication services
    /// </summary>
    public IdentityAuthenticationBuilder AddAll()
    {
        return AddAuthorization().AddJwtBearer();
    }

    /// <summary>
    /// Return to main builder
    /// </summary>
    public IdentityPrvdBuilder And()
    {
        return new IdentityPrvdBuilder(_services, _options);
    }
} 