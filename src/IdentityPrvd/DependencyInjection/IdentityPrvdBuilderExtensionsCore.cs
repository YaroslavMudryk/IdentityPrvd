using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Helpers;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Endpoints;
using IdentityPrvd.Features.Authentication.ChangeLogin;
using IdentityPrvd.Features.Authentication.ChangePassword;
using IdentityPrvd.Features.Authentication.ExternalSignin;
using IdentityPrvd.Features.Authentication.LinkExternalSignin;
using IdentityPrvd.Features.Authentication.QrSignin;
using IdentityPrvd.Features.Authentication.RestorePassword;
using IdentityPrvd.Features.Authentication.Signin;
using IdentityPrvd.Features.Authentication.SigninOptions;
using IdentityPrvd.Features.Authentication.Signout;
using IdentityPrvd.Features.Authentication.Signup;
using IdentityPrvd.Features.Authorization.Claims;
using IdentityPrvd.Features.Authorization.Roles;
using IdentityPrvd.Features.Personal.Contacts;
using IdentityPrvd.Features.Personal.Devices;
using IdentityPrvd.Features.Security.Initialize;
using IdentityPrvd.Features.Security.Mfa.DisableMfa;
using IdentityPrvd.Features.Security.Mfa.EnableMfa;
using IdentityPrvd.Features.Security.RefreshToken;
using IdentityPrvd.Features.Security.Sessions.GetSessions;
using IdentityPrvd.Features.Security.Sessions.RevokeSessions;
using IdentityPrvd.Infrastructure.Caching;
using IdentityPrvd.Infrastructure.Database.Context;
using IdentityPrvd.Infrastructure.Database.Transactions;
using IdentityPrvd.Infrastructure.Middleware;
using IdentityPrvd.Services.AuthSchemes;
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
                    ValidateAudience = true,
                    ValidAudience = options.Token.Audience,
                    ValidateLifetime = true,
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
        builder.Services.AddSignoutDependencies();
        builder.Services.AddGetSessionsDependencies();
        builder.Services.AddRevokeSessionsDependencies();
        builder.Services.AddEnableMfaDependencies();
        builder.Services.AddDisableMfaDependencies();
        builder.Services.AddRolesDependencies();
        builder.Services.AddClaimsDependencies();
        builder.Services.AddExternalSigninDependencies();
        builder.Services.AddLinkExternalProviderDependencies();
        builder.Services.AddChangeLoginDependencies();
        builder.Services.AddChangePasswordDependencies();
        builder.Services.AddRestorePasswordDependencies();
        builder.Services.AddContactsDependencies();
        builder.Services.AddDevicesDependencies();
        builder.Services.AddQrSigninDependencies();
        builder.Services.AddInitializeDependencies();
        return builder;
    }

    internal static IIdentityPrvdBuilder AddRequiredServices(this IIdentityPrvdBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<UserHelper>();
        builder.Services.TryAddSingleton(TimeProvider.System);
        builder.Services.AddScoped<IAuthSchemes, DefaultAuthSchemes>();
        builder.Services.AddScoped<IMfaService, TotpMfaService>();
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<ISystemStatus, DefaultSystemStatus>();
        return builder;
    }

    internal static IIdentityPrvdBuilder AddSessionServices(this IIdentityPrvdBuilder builder)
    {
        builder.Services.AddScoped<ISessionManager, SessionManager>();
        builder.Services.AddTransient<ServerSideSessionMiddleware>();
        return builder;
    }

    internal static IIdentityPrvdBuilder AddContexts(this IIdentityPrvdBuilder builder)
    {
        builder.Services.AddScoped<IUserContext, UserContext>();
        builder.Services.AddScoped<ICurrentContext, CurrentContext>();
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
            EfClaimStore,
            EfClientClaimStore,
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
            EfRoleClaimStore,
            EfRoleStore,
            EfSessionStore,
            EfUserLoginStore,
            EfUserRoleStore,
            EfUserStore>(builder);
    }

    public static IIdentityPrvdBuilder UseEfQueries(this IIdentityPrvdBuilder builder)
    {
        return UseQueries<EfBansQuery,
            EfClaimsQuery,
            EfClientClaimsQuery,
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
            EfRoleClaimsQuery,
            EfRolesQuery,
            EfSessionsQuery,
            EfUserLoginsQuery,
            EfUserRolesQuery,
            EfUsersQuery>(builder);
    }
}
