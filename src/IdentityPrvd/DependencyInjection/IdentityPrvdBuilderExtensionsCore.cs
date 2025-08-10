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
using IdentityPrvd.Services.AuthSchemes;
using IdentityPrvd.Services.Security;
using IdentityPrvd.Services.ServerSideSessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Redis.OM;
using Redis.OM.Contracts;

namespace IdentityPrvd.DependencyInjection;

public static class IdentityPrvdBuilderExtensionsCore
{
    public static IIdentityPrvdBuilder AddEndpoints(this IIdentityPrvdBuilder builder)
    {
        builder.Services.AddEndpoints();

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

    public static IIdentityPrvdBuilder AddSessionServices(this IIdentityPrvdBuilder builder)
    {
        builder.Services.AddScoped<ISessionManager, SessionManager>();
        builder.Services.AddScoped<Infrastructure.Caching.ISessionStore, RedisSessionStore>();
        builder.Services.AddScoped<IRedisConnectionProvider>(provider =>
        {
            return new RedisConnectionProvider(builder.Option.Connections.Redis);
        });
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

    public static IIdentityPrvdBuilder AddTransaction<TTransactionManager>(this IIdentityPrvdBuilder builder) where TTransactionManager : class, ITransactionManager
    {
        builder.Services.AddScoped<ITransactionManager, TTransactionManager>();
        return builder;
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

    public static IIdentityPrvdBuilder AddStores<
        TBanStore,
        TClaimStore,
        TClientClaimStore,
        TClientSecretStore,
        TClientStore,
        TConfirmStore,
        TContactStore,
        TDeviceStore,
        TFailedLoginAttemptStore,
        TMfaRecoveryCodeStore,
        TMfaStore,
        TPasswordStore,
        TQrStore,
        TRefreshTokenStore,
        TRoleClaimStore,
        TRoleStore,
        TSessionStore,
        TUserLoginStore,
        TUserRoleStore,
        TUserStore>(this IIdentityPrvdBuilder builder)
        where TBanStore : class, IBanStore
        where TClaimStore : class, IClaimStore
        where TClientClaimStore : class, IClientClaimStore
        where TClientSecretStore : class, IClientSecretStore
        where TClientStore : class, IClientStore
        where TConfirmStore : class, IConfirmStore
        where TContactStore : class, IContactStore
        where TDeviceStore : class, IDeviceStore
        where TFailedLoginAttemptStore : class, IFailedLoginAttemptStore
        where TMfaRecoveryCodeStore : class, IMfaRecoveryCodeStore
        where TMfaStore : class, IMfaStore
        where TPasswordStore : class, IPasswordStore
        where TQrStore : class, IQrStore
        where TRefreshTokenStore : class, IRefreshTokenStore
        where TRoleClaimStore : class, IRoleClaimStore
        where TRoleStore : class, IRoleStore
        where TSessionStore : class, Data.Stores.ISessionStore
        where TUserLoginStore : class, IUserLoginStore
        where TUserRoleStore : class, IUserRoleStore
        where TUserStore : class, IUserStore
    {
        builder.Services.AddScoped<IBanStore, TBanStore>();
        builder.Services.AddScoped<IClaimStore, TClaimStore>();
        builder.Services.AddScoped<IClientClaimStore, TClientClaimStore>();
        builder.Services.AddScoped<IClientSecretStore, TClientSecretStore>();
        builder.Services.AddScoped<IClientStore, TClientStore>();
        builder.Services.AddScoped<IConfirmStore, TConfirmStore>();
        builder.Services.AddScoped<IContactStore, TContactStore>();
        builder.Services.AddScoped<IDeviceStore, TDeviceStore>();
        builder.Services.AddScoped<IFailedLoginAttemptStore, TFailedLoginAttemptStore>();
        builder.Services.AddScoped<IMfaRecoveryCodeStore, TMfaRecoveryCodeStore>();
        builder.Services.AddScoped<IMfaStore, TMfaStore>();
        builder.Services.AddScoped<IPasswordStore, TPasswordStore>();
        builder.Services.AddScoped<IQrStore, TQrStore>();
        builder.Services.AddScoped<IRefreshTokenStore, TRefreshTokenStore>();
        builder.Services.AddScoped<IRoleClaimStore, TRoleClaimStore>();
        builder.Services.AddScoped<IRoleStore, TRoleStore>();
        builder.Services.AddScoped<Data.Stores.ISessionStore, TSessionStore>();
        builder.Services.AddScoped<IUserLoginStore, TUserLoginStore>();
        builder.Services.AddScoped<IUserRoleStore, TUserRoleStore>();
        builder.Services.AddScoped<IUserStore, TUserStore>();
        return builder;
    }

    public static IIdentityPrvdBuilder AddQueries<
        TBansQuery,
        TClaimsQuery,
        TClientClaimsQuery,
        TClientSecretsQuery,
        TClientsQuery,
        TConfirmsQuery,
        TContactsQuery,
        TDevicesQuery,
        TFailedLoginAttemptsQuery,
        TMfaRecoveryCodesQuery,
        TMfasQuery,
        TPasswordsQuery,
        TQrsQuery,
        TRefreshTokensQuery,
        TRoleClaimsQuery,
        TRolesQuery,
        TSessionsQuery,
        TUserLoginsQuery,
        TUserRolesQuery,
        TUsersQuery>(this IIdentityPrvdBuilder builder)
        where TBansQuery : class, IBansQuery
        where TClaimsQuery : class, IClaimsQuery
        where TClientClaimsQuery : class, IClientClaimsQuery
        where TClientSecretsQuery : class, IClientSecretsQuery
        where TClientsQuery : class, IClientsQuery
        where TConfirmsQuery : class, IConfirmsQuery
        where TContactsQuery : class, IContactsQuery
        where TDevicesQuery : class, IDevicesQuery
        where TFailedLoginAttemptsQuery : class, IFailedLoginAttemptsQuery
        where TMfaRecoveryCodesQuery : class, IMfaRecoveryCodesQuery
        where TMfasQuery : class, IMfasQuery
        where TPasswordsQuery : class, IPasswordsQuery
        where TQrsQuery : class, IQrsQuery
        where TRefreshTokensQuery : class, IRefreshTokensQuery
        where TRoleClaimsQuery : class, IRoleClaimsQuery
        where TRolesQuery : class, IRolesQuery
        where TSessionsQuery : class, ISessionsQuery
        where TUserLoginsQuery : class, IUserLoginsQuery
        where TUserRolesQuery : class, IUserRolesQuery
        where TUsersQuery : class, IUsersQuery
    {
        builder.Services.AddScoped<IBansQuery, TBansQuery>();
        builder.Services.AddScoped<IClaimsQuery, TClaimsQuery>();
        builder.Services.AddScoped<IClientClaimsQuery, TClientClaimsQuery>();
        builder.Services.AddScoped<IClientSecretsQuery, TClientSecretsQuery>();
        builder.Services.AddScoped<IClientsQuery, TClientsQuery>();
        builder.Services.AddScoped<IConfirmsQuery, TConfirmsQuery>();
        builder.Services.AddScoped<IContactsQuery, TContactsQuery>();
        builder.Services.AddScoped<IDevicesQuery, TDevicesQuery>();
        builder.Services.AddScoped<IFailedLoginAttemptsQuery, TFailedLoginAttemptsQuery>();
        builder.Services.AddScoped<IMfaRecoveryCodesQuery, TMfaRecoveryCodesQuery>();
        builder.Services.AddScoped<IMfasQuery, TMfasQuery>();
        builder.Services.AddScoped<IPasswordsQuery, TPasswordsQuery>();
        builder.Services.AddScoped<IQrsQuery, TQrsQuery>();
        builder.Services.AddScoped<IRefreshTokensQuery, TRefreshTokensQuery>();
        builder.Services.AddScoped<IRoleClaimsQuery, TRoleClaimsQuery>();
        builder.Services.AddScoped<IRolesQuery, TRolesQuery>();
        builder.Services.AddScoped<ISessionsQuery, TSessionsQuery>();
        builder.Services.AddScoped<IUserLoginsQuery, TUserLoginsQuery>();
        builder.Services.AddScoped<IUserRolesQuery, TUserRolesQuery>();
        builder.Services.AddScoped<IUsersQuery, TUsersQuery>();
        return builder;
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

    public static IIdentityPrvdBuilder AddDbContext<DbContext>(this IIdentityPrvdBuilder builder, Action<DbContextOptionsBuilder> optionsAction) where DbContext : IdentityPrvdContext
    {
        builder.Services.AddDbContext<DbContext>(optionsAction);
        return builder;
    }

    public static IIdentityPrvdBuilder AddDefaultContext(this IIdentityPrvdBuilder builder)
    {
        return AddDbContext<IdentityPrvdContext>(builder, options =>
        {
            options.UseNpgsql(builder.Option.Connections.Db);
        });
    }
}
