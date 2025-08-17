using IdentityPrvd.Common.Helpers;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Data.Stores;
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
using IdentityPrvd.Services.AuthSchemes;
using IdentityPrvd.Services.Location;
using IdentityPrvd.Services.Notification;
using IdentityPrvd.Services.Security;
using IdentityPrvd.Services.ServerSideSessions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Redis.OM;
using Redis.OM.Contracts;
using System.Text;

namespace IdentityPrvd.DependencyInjection;

public static partial class IdentityPrvdBuilderExtensionsCore
{
    public static IIdentityPrvdBuilder AddEndpoints(this IIdentityPrvdBuilder builder)
    {
        builder.Services.AddEndpoints();
        return builder;
    }

    public static IIdentityPrvdBuilder AddAuthentication(this IIdentityPrvdBuilder builder)
    {
        var services = builder.Services;
        var options = builder.Option;

        var authBuilder = services.AddAuthentication();
        authBuilder
            .AddCookie("cookie")
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

        services.AddAuthorization();

        return builder;
    }

    public static IIdentityPrvdBuilder AddCoreServices(this IIdentityPrvdBuilder builder)
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
        return builder;
    }

    public static IIdentityPrvdBuilder AddRequiredServices(this IIdentityPrvdBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<UserHelper>();
        builder.Services.TryAddSingleton(TimeProvider.System);
        builder.Services.AddScoped<IAuthSchemes, DefaultAuthSchemes>();
        return builder;
    }

    public static IIdentityPrvdBuilder AddProtectionServices(this IIdentityPrvdBuilder builder)
    {
        builder.Services.AddScoped<IProtectionService, AesProtectionService>();
        builder.Services.AddScoped<IMfaService, TotpMfaService>();
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IHasher, Sha512Hasher>();
        return builder;
    }

    public static IIdentityPrvdBuilder AddInMemorySessionManagerStore(this IIdentityPrvdBuilder builder)
    {
        return AddSessionManagerStore<InMemorySessionManagerStore>(builder);
    }

    public static IIdentityPrvdBuilder AddRedisSessionManagerStore(this IIdentityPrvdBuilder builder)
    {
        builder.Services.AddScoped<IRedisConnectionProvider>(provider =>
        {
            return new RedisConnectionProvider(builder.Option.Connections.Redis);
        });
        return AddSessionManagerStore<RedisSessionManagerStore>(builder);
    }

    public static IIdentityPrvdBuilder AddSessionServices(this IIdentityPrvdBuilder builder)
    {
        builder.Services.AddScoped<ISessionManager, SessionManager>();
        builder.Services.AddTransient<ServerSideSessionMiddleware>();
        return builder;
    }

    public static IIdentityPrvdBuilder AddContexts(this IIdentityPrvdBuilder builder)
    {
        builder.Services.AddScoped<IUserContext, UserContext>();
        builder.Services.AddScoped<ICurrentContext, CurrentContext>();
        return builder;
    }

    public static IIdentityPrvdBuilder AddMiddlewares(this IIdentityPrvdBuilder builder)
    {
        builder.Services.AddTransient<CorrelationContextMiddleware>();
        builder.Services.AddTransient<ServerSideSessionMiddleware>();
        builder.Services.AddTransient<GlobalExceptionHandlerMiddleware>();
        return builder;
    }

    public static IIdentityPrvdBuilder AddFakeSmsNotifier(this IIdentityPrvdBuilder builder)
    {
        return AddSmsNotifier<FakeSmsService>(builder);
    }

    public static IIdentityPrvdBuilder AddFakeEmailNotifier(this IIdentityPrvdBuilder builder)
    {
        return AddEmailNotifier<FakeEmailService>(builder);
    }

    public static IIdentityPrvdBuilder AddFakeLocationService(this IIdentityPrvdBuilder builder)
    {
        return AddLocationService<FakeLocationService>(builder);
    }

    public static IIdentityPrvdBuilder AddIpApiLocationService(this IIdentityPrvdBuilder builder)
    {
        builder.Services.AddHttpClient<IpApiLocationService>(options =>
        {
            options.BaseAddress = new Uri("http://ip-api.com");
        });
        return AddLocationService<IpApiLocationService>(builder);
    }

    public static IIdentityPrvdBuilder AddEfTransaction(this IIdentityPrvdBuilder builder)
    {
        return AddTransaction<EfCoreTransactionManager>(builder);
    }

    public static IIdentityPrvdBuilder AddEfStores(this IIdentityPrvdBuilder builder)
    {
        return AddStores<
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

    public static IIdentityPrvdBuilder AddEfQueries(this IIdentityPrvdBuilder builder)
    {
        return AddQueries<EfBansQuery,
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

    public static IIdentityPrvdBuilder AddDefaultDbContext(this IIdentityPrvdBuilder builder)
    {
        return AddDbContext<IdentityPrvdContext>(builder, builder.Option.Connections.Db);
    }
}
