using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Helpers;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Endpoints;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection.Extensions;
using IdentityPrvd.Features.Authentication.ChangeLogin;
using IdentityPrvd.Features.Authentication.ChangePassword;
using IdentityPrvd.Features.Authentication.ExternalSignin;
using IdentityPrvd.Features.Authentication.LinkExternalSignin;
using IdentityPrvd.Features.Authentication.QrSignin;
using IdentityPrvd.Features.Authentication.RestorePassword;
using IdentityPrvd.Features.Authentication.Signin;
using IdentityPrvd.Features.Authentication.SigninOptions;
using IdentityPrvd.Features.Authentication.Signup;
using IdentityPrvd.Features.Authorization.Clients;
using IdentityPrvd.Features.Authorization.Permissions;
using IdentityPrvd.Features.Authorization.Roles;
using IdentityPrvd.Features.Personal.Contacts;
using IdentityPrvd.Features.Personal.Devices;
using IdentityPrvd.Features.Security.Initialize;
using IdentityPrvd.Features.Security.Mfa.DisableMfa;
using IdentityPrvd.Features.Security.Mfa.EnableMfa;
using IdentityPrvd.Features.Security.RefreshToken;
using IdentityPrvd.Features.Security.Sessions.GetSession;
using IdentityPrvd.Features.Security.Sessions.GetSessions;
using IdentityPrvd.Features.Security.Sessions.RevokeSessions;
using IdentityPrvd.Features.Shared.Services;
using IdentityPrvd.Infrastructure.Caching;
using IdentityPrvd.Infrastructure.Database.Context;
using IdentityPrvd.Infrastructure.Database.Transactions;
using IdentityPrvd.Infrastructure.Middleware;
using IdentityPrvd.Services.AuthSchemes;
using IdentityPrvd.Services.Localization;
using IdentityPrvd.Services.Location;
using IdentityPrvd.Services.Notification;
using IdentityPrvd.Services.Security;
using IdentityPrvd.Services.ServerSideSessions;
using IdentityPrvd.Services.SystemStatus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Redis.OM;
using Redis.OM.Contracts;
using System.Text;

namespace IdentityPrvd.DependencyInjection;

public static partial class IdentityPrvdBuilderExtensionsCore
{
    internal static IIdentityPrvdBuilder AddEndpoints(this IIdentityPrvdBuilder builder)
    {
        builder.Services.AddEndpoints();
        return builder;
    }

    internal static IIdentityPrvdBuilder AddAuthentication(this IIdentityPrvdBuilder builder)
    {
        var services = builder.Services;
        var options = builder.Options;
        services.AddAuthorization();

        builder.AuthenticationBuilder
            .AddCookie(AppConstants.DefaultExternalProviderScheme)
            .AddJwtBearer(jwt =>
            {
                jwt.RequireHttpsMetadata = false;
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ClockSkew = TimeSpan.Zero,
                    ValidateIssuer = true,
                    ValidIssuer = options.Token.Issuer,
                    ValidateLifetime = true,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(options.Token.SecretKey!)),
                    ValidateIssuerSigningKey = true,
                };
                jwt.SaveToken = true;
            });

        services.AddScoped<ExternalProviderManager>();
        return builder;
    }

    internal static IIdentityPrvdBuilder AddCoreServices(this IIdentityPrvdBuilder builder)
    {
        builder.Services.AddSignupDependencies();
        builder.Services.AddSigninDependencies();
        builder.Services.AddSigninOptionsDependencies();
        builder.Services.AddRefreshTokenDependencies();
        builder.Services.AddGetSessionsDependencies();
        builder.Services.AddGetSessionDependencies();
        builder.Services.AddRevokeSessionsDependencies();
        builder.Services.AddEnableMfaDependencies();
        builder.Services.AddDisableMfaDependencies();
        builder.Services.AddRolesDependencies();
        builder.Services.AddPermissionsDependencies();
        builder.Services.AddExternalSigninDependencies();
        builder.Services.AddLinkExternalProviderDependencies();
        builder.Services.AddChangeLoginDependencies();
        builder.Services.AddChangePasswordDependencies();
        builder.Services.AddRestorePasswordDependencies();
        builder.Services.AddContactsDependencies();
        builder.Services.AddDevicesDependencies();
        builder.Services.AddQrSigninDependencies();
        builder.Services.AddInitializeDependencies();
        builder.Services.AddClientsDependencies();
        return builder;
    }

    internal static IIdentityPrvdBuilder AddRequiredServices(this IIdentityPrvdBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        if (builder.UseMemoryCache)
        {
            builder.Services.AddMemoryCache();
        }
        builder.Services.AddScoped<UserHelper>();
        builder.Services.TryAddSingleton(TimeProvider.System);
        builder.Services.AddScoped<IAuthSchemes, DefaultAuthSchemes>();
        builder.Services.AddScoped<IMfaService, TotpMfaService>();
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<ISystemStatus, DefaultSystemStatus>();
        builder.Services.AddScoped<ISessionControlService, SessionControlService>();
        builder.Services.AddSingleton<ILocalizationService, LocalizationService>();
        return builder;
    }

    internal static IIdentityPrvdBuilder AddSessionServices(this IIdentityPrvdBuilder builder)
    {
        builder.Services.AddScoped<ISessionManager, SessionManager>();
        builder.Services.AddTransient<ServerSideSessionMiddleware>();
        return builder;
    }

    internal static IIdentityPrvdBuilder AddContext(this IIdentityPrvdBuilder builder)
    {
        builder.Services.AddScoped<IIdentityContext, IdentityContext>();
        return builder;
    }

    internal static IIdentityPrvdBuilder AddDefaultDbContext(this IIdentityPrvdBuilder builder)
    {
        return UseDbContext<IdentityPrvdContext>(builder, options =>
        {
            options.UseNpgsql(builder.Options.Connections.Db);
        });
    }

    internal static IIdentityPrvdBuilder AddMiddlewares(this IIdentityPrvdBuilder builder)
    {
        builder.Services.AddTransient<CorrelationContextMiddleware>();
        builder.Services.AddTransient<LanguageDetectionMiddleware>();
        builder.Services.AddTransient<ServerSideSessionMiddleware>();
        builder.Services.AddTransient<GlobalExceptionHandlerMiddleware>();
        return builder;
    }

    public static IIdentityPrvdBuilder UseFakeProtectionService(this IIdentityPrvdBuilder builder)
    {
        return UseProtectionService<FakeProtectionService>(builder);
    }

    public static IIdentityPrvdBuilder UseAesProtectionService(this IIdentityPrvdBuilder builder)
    {
        return UseProtectionService<AesProtectionService>(builder);
    }

    public static IIdentityPrvdBuilder UseFakeHasher(this IIdentityPrvdBuilder builder)
    {
        return UseHasher<FakeHasher>(builder);
    }

    public static IIdentityPrvdBuilder UseSha512Hasher(this IIdentityPrvdBuilder builder)
    {
        return UseHasher<Sha512Hasher>(builder);
    }

    public static IIdentityPrvdBuilder UseInMemorySessionManagerStore(this IIdentityPrvdBuilder builder)
    {
        return UseSessionManagerStore<InMemorySessionManagerStore>(builder);
    }

    public static IIdentityPrvdBuilder UseMemoryCache(this IIdentityPrvdBuilder builder)
    {
        builder.UseMemoryCache = true;
        return builder;
    }

    public static IIdentityPrvdBuilder UseRedisSessionManagerStore(this IIdentityPrvdBuilder builder)
    {
        return UseRedisSessionManagerStore(builder, builder.Options.Connections.Redis);
    }

    public static IIdentityPrvdBuilder UseRedisSessionManagerStore(this IIdentityPrvdBuilder builder, string redisConnection)
    {
        ArgumentNullException.ThrowIfNull(redisConnection);

        builder.Services.AddScoped<IRedisConnectionProvider>(provider =>
        {
            return new RedisConnectionProvider(redisConnection);
        });
        return UseSessionManagerStore<RedisSessionManagerStore>(builder);
    }

    public static IIdentityPrvdBuilder UseFakeSmsNotifier(this IIdentityPrvdBuilder builder)
    {
        return UseSmsNotifier<FakeSmsService>(builder);
    }

    public static IIdentityPrvdBuilder UseFakeEmailNotifier(this IIdentityPrvdBuilder builder)
    {
        return UseEmailNotifier<FakeEmailService>(builder);
    }

    public static IIdentityPrvdBuilder UseFakeLocationService(this IIdentityPrvdBuilder builder)
    {
        return UseLocationService<FakeLocationService>(builder);
    }

    public static IIdentityPrvdBuilder UseIpApiLocationService(this IIdentityPrvdBuilder builder)
    {
        builder.Services.AddHttpClient("IpApiLocation", options =>
        {
            options.BaseAddress = new Uri("http://ip-api.com/");
        });
        return UseLocationService<IpApiLocationService>(builder);
    }

    public static IIdentityPrvdBuilder UseEfTransaction(this IIdentityPrvdBuilder builder)
    {
        return UseTransaction<EfCoreTransactionManager>(builder);
    }

    public static IIdentityPrvdBuilder UseEfStores(this IIdentityPrvdBuilder builder)
    {
        return UseStores<
            EfBanStore,
            EfPermissionStore,
            EfClientPermissionStore,
            EfClientSecretStore,
            EfClientStore,
            EfConfirmStore,
            EfContactStore,
            EfDeviceStore,
            EfFailedLoginAttemptStore,
            EfMfaRecoveryCodeStore,
            EfMfaStore,
            EfPasswordStore,
            EfQrStore,
            EfRefreshTokenStore,
            EfRolePermissionStore,
            EfRoleStore,
            EfSessionStore,
            EfUserLoginStore,
            EfUserRoleStore,
            EfUserStore>(builder);
    }

    public static IIdentityPrvdBuilder UseEfQueries(this IIdentityPrvdBuilder builder)
    {
        // Register base EF queries
        UseQueries<EfBansQuery,
            EfPermissionsQuery,
            EfClientPermissionsQuery,
            EfClientSecretsQuery,
            EfClientsQuery,
            EfConfirmsQuery,
            EfContactsQuery,
            EfDevicesQuery,
            EfFailedLoginAttemptsQuery,
            EfMfaRecoveryCodesQuery,
            EfMfasQuery,
            EfPasswordsQuery,
            EfQrsQuery,
            EfRefreshTokensQuery,
            EfRolePermissionsQuery,
            EfRolesQuery,
            EfSessionsQuery,
            EfUserLoginsQuery,
            EfUserRolesQuery,
            EfUsersQuery>(builder);

        // Wrap frequently accessed queries with caching if enabled
        if (builder.UseMemoryCache)
        {
            WrapQueriesWithCache(builder);
        }
        
        return builder;
    }

    private static void WrapQueriesWithCache(IIdentityPrvdBuilder builder)
    {
        // Wrap IUserRolesQuery with cached version
        WrapQueryWithCache<IUserRolesQuery, CachedUserRolesQuery>(builder, 
            (inner, cache) => new CachedUserRolesQuery(inner, cache));

        // Wrap IRolePermissionsQuery with cached version
        WrapQueryWithCache<IRolePermissionsQuery, CachedRolePermissionsQuery>(builder,
            (inner, cache) => new CachedRolePermissionsQuery(inner, cache));

        // Wrap IClientPermissionsQuery with cached version
        WrapQueryWithCache<IClientPermissionsQuery, CachedClientPermissionsQuery>(builder,
            (inner, cache) => new CachedClientPermissionsQuery(inner, cache));

        // Wrap IClientsQuery with cached version
        WrapQueryWithCache<IClientsQuery, CachedClientsQuery>(builder,
            (inner, cache) => new CachedClientsQuery(inner, cache));
    }

    private static void WrapQueryWithCache<TInterface, TCached>(
        IIdentityPrvdBuilder builder,
        Func<TInterface, IMemoryCache, TCached> factory)
        where TInterface : class
        where TCached : class, TInterface
    {
        var descriptor = builder.Services.FirstOrDefault(s => s.ServiceType == typeof(TInterface));
        if (descriptor == null)
            return;

        // Save the original descriptor information before removing it
        var originalFactory = descriptor.ImplementationFactory;
        var originalInstance = descriptor.ImplementationInstance;
        var originalType = descriptor.ImplementationType;
        var lifetime = descriptor.Lifetime;

        // Remove the original registration
        builder.Services.Remove(descriptor);

        // Register the cached wrapper
        if (originalType != null)
        {
            // If registered by type, register the type separately and wrap it
            builder.Services.Add(new ServiceDescriptor(originalType, originalType, lifetime));
            
            builder.Services.Add(new ServiceDescriptor(typeof(TInterface), provider =>
            {
                var inner = (TInterface)provider.GetRequiredService(originalType);
                var cache = provider.GetRequiredService<IMemoryCache>();
                return factory(inner, cache);
            }, lifetime));
        }
        else if (originalFactory != null)
        {
            // If registered by factory, wrap the factory
            builder.Services.Add(new ServiceDescriptor(typeof(TInterface), provider =>
            {
                var inner = (TInterface)originalFactory(provider);
                var cache = provider.GetRequiredService<IMemoryCache>();
                return factory(inner, cache);
            }, lifetime));
        }
        else if (originalInstance != null)
        {
            // If registered by instance, wrap the instance
            builder.Services.Add(new ServiceDescriptor(typeof(TInterface), provider =>
            {
                var inner = (TInterface)originalInstance;
                var cache = provider.GetRequiredService<IMemoryCache>();
                return factory(inner, cache);
            }, lifetime));
        }
    }
}
