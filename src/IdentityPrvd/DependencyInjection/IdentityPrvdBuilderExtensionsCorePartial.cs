using IdentityPrvd.Data.Queries;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Data.Transactions;
using IdentityPrvd.Infrastructure.Caching;
using IdentityPrvd.Infrastructure.Database.Context;
using IdentityPrvd.Services.Location;
using IdentityPrvd.Services.Notification;
using IdentityPrvd.Services.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.DependencyInjection;

public static partial class IdentityPrvdBuilderExtensionsCore
{
    private static IIdentityPrvdBuilder AddService<TInterface, TImplementation>(this IIdentityPrvdBuilder builder, ServiceLifetime lifetime) where TInterface : class where TImplementation : class, TInterface
    {
        var serviceDescriptor = builder.Services.FirstOrDefault(s => s.ServiceType == typeof(TInterface));
        if (serviceDescriptor != null)
            builder.Services.Remove(serviceDescriptor);

        if (lifetime == ServiceLifetime.Singleton)
        {
            builder.Services.AddSingleton<TInterface, TImplementation>();
        }
        else if (lifetime == ServiceLifetime.Transient)
        {
            builder.Services.AddTransient<TInterface, TImplementation>();
        }
        else
        {
            builder.Services.AddScoped<TInterface, TImplementation>();
        }

        return builder;
    }

    public static IIdentityPrvdBuilder UseSessionManagerStore<TSessionManagerStore>(this IIdentityPrvdBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Scoped) where TSessionManagerStore : class, ISessionManagerStore
    {
        return AddService<ISessionManagerStore, TSessionManagerStore>(builder, lifetime);
    }

    public static IIdentityPrvdBuilder UseSmsNotifier<TSmsNotifier>(this IIdentityPrvdBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Scoped) where TSmsNotifier : class, ISmsService
    {
        return AddService<ISmsService, TSmsNotifier>(builder, lifetime);
    }

    public static IIdentityPrvdBuilder UseEmailNotifier<TEmailNotifier>(this IIdentityPrvdBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Scoped) where TEmailNotifier : class, IEmailService
    {
        return AddService<IEmailService, TEmailNotifier>(builder, lifetime);
    }

    public static IIdentityPrvdBuilder UseLocationService<TLocationService>(this IIdentityPrvdBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Scoped) where TLocationService : class, ILocationService
    {
        return AddService<ILocationService, TLocationService>(builder, lifetime);
    }

    public static IIdentityPrvdBuilder UseProtectionService<TProtectionService>(this IIdentityPrvdBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Scoped) where TProtectionService : class, IProtectionService
    {
        return AddService<IProtectionService, TProtectionService>(builder, lifetime);
    }

    public static IIdentityPrvdBuilder UseHasher<THasher>(this IIdentityPrvdBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Scoped) where THasher : class, IHasher
    {
        return AddService<IHasher, THasher>(builder, lifetime);
    }

    public static IIdentityPrvdBuilder UseTransaction<TTransactionManager>(this IIdentityPrvdBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Scoped) where TTransactionManager : class, ITransactionManager
    {
        return AddService<ITransactionManager, TTransactionManager>(builder, lifetime);
    }

    public static IIdentityPrvdBuilder UseStores<
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
        TUserStore>(this IIdentityPrvdBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Scoped)
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
    where TSessionStore : class, ISessionStore
    where TUserLoginStore : class, IUserLoginStore
    where TUserRoleStore : class, IUserRoleStore
    where TUserStore : class, IUserStore
    {
        AddService<IBanStore, TBanStore>(builder, lifetime);
        AddService<IClaimStore, TClaimStore>(builder, lifetime);
        AddService<IClientClaimStore, TClientClaimStore>(builder, lifetime);
        AddService<IClientSecretStore, TClientSecretStore>(builder, lifetime);
        AddService<IClientStore, TClientStore>(builder, lifetime);
        AddService<IConfirmStore, TConfirmStore>(builder, lifetime);
        AddService<IContactStore, TContactStore>(builder, lifetime);
        AddService<IDeviceStore, TDeviceStore>(builder, lifetime);
        AddService<IFailedLoginAttemptStore, TFailedLoginAttemptStore>(builder, lifetime);
        AddService<IMfaRecoveryCodeStore, TMfaRecoveryCodeStore>(builder, lifetime);
        AddService<IMfaStore, TMfaStore>(builder, lifetime);
        AddService<IPasswordStore, TPasswordStore>(builder, lifetime);
        AddService<IQrStore, TQrStore>(builder, lifetime);
        AddService<IRefreshTokenStore, TRefreshTokenStore>(builder, lifetime);
        AddService<IRoleClaimStore, TRoleClaimStore>(builder, lifetime);
        AddService<IRoleStore, TRoleStore>(builder, lifetime);
        AddService<ISessionStore, TSessionStore>(builder, lifetime);
        AddService<IUserLoginStore, TUserLoginStore>(builder, lifetime);
        AddService<IUserRoleStore, TUserRoleStore>(builder, lifetime);
        AddService<IUserStore, TUserStore>(builder, lifetime);
        return builder;
    }

    public static IIdentityPrvdBuilder UseQueries<
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
        TUsersQuery>(this IIdentityPrvdBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Scoped)
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
        AddService<IBansQuery, TBansQuery>(builder, lifetime);
        AddService<IClaimsQuery, TClaimsQuery>(builder, lifetime);
        AddService<IClientClaimsQuery, TClientClaimsQuery>(builder, lifetime);
        AddService<IClientSecretsQuery, TClientSecretsQuery>(builder, lifetime);
        AddService<IClientsQuery, TClientsQuery>(builder, lifetime);
        AddService<IConfirmsQuery, TConfirmsQuery>(builder, lifetime);
        AddService<IContactsQuery, TContactsQuery>(builder, lifetime);
        AddService<IDevicesQuery, TDevicesQuery>(builder, lifetime);
        AddService<IFailedLoginAttemptsQuery, TFailedLoginAttemptsQuery>(builder, lifetime);
        AddService<IMfaRecoveryCodesQuery, TMfaRecoveryCodesQuery>(builder, lifetime);
        AddService<IMfasQuery, TMfasQuery>(builder, lifetime);
        AddService<IPasswordsQuery, TPasswordsQuery>(builder, lifetime);
        AddService<IQrsQuery, TQrsQuery>(builder, lifetime);
        AddService<IRefreshTokensQuery, TRefreshTokensQuery>(builder, lifetime);
        AddService<IRoleClaimsQuery, TRoleClaimsQuery>(builder, lifetime);
        AddService<IRolesQuery, TRolesQuery>(builder, lifetime);
        AddService<ISessionsQuery, TSessionsQuery>(builder, lifetime);
        AddService<IUserLoginsQuery, TUserLoginsQuery>(builder, lifetime);
        AddService<IUserRolesQuery, TUserRolesQuery>(builder, lifetime);
        AddService<IUsersQuery, TUsersQuery>(builder, lifetime);
        return builder;
    }

    public static IIdentityPrvdBuilder UseDbContext<DbContext>(this IIdentityPrvdBuilder builder, Action<DbContextOptionsBuilder> optionsAction) where DbContext : IdentityPrvdContext
    {
        builder.Services.AddDbContext<DbContext>(optionsAction);
        return builder;
    }

    public static IIdentityPrvdBuilder UseExternalProviders(this IIdentityPrvdBuilder builder)
    {
        var identityOptions = builder.Options;
        var authBuilder = builder.AuthenticationBuilder;

        if (identityOptions.ExternalProviders.TryGetValue("Google", out var googleOptions))
        {
            authBuilder.AddGoogle(options =>
            {
                options.ClientId = googleOptions.ClientId;
                options.ClientSecret = googleOptions.ClientSecret;
                options.CallbackPath = "/signin-google";
                options.SignInScheme = "cookie";
                options.SaveTokens = true;
            });
        }

        if (identityOptions.ExternalProviders.TryGetValue("Microsoft", out var microsoftOptions))
        {
            authBuilder.AddMicrosoftAccount(options =>
            {
                options.ClientId = microsoftOptions.ClientId;
                options.ClientSecret = microsoftOptions.ClientSecret;
                options.CallbackPath = "/signin-microsoft";
                options.SignInScheme = "cookie";
                options.SaveTokens = true;
            });
        }

        if (identityOptions.ExternalProviders.TryGetValue("GitHub", out var githubOptions))
        {
            authBuilder.AddGitHub(options =>
            {
                options.ClientId = githubOptions.ClientId;
                options.ClientSecret = githubOptions.ClientSecret;
                options.CallbackPath = "/signin-github";
                options.SignInScheme = "cookie";
                options.Scope.Add("read:user");
                options.Scope.Add("user:email");
                options.SaveTokens = true;
            });
        }

        if (identityOptions.ExternalProviders.TryGetValue("Facebook", out var facebookOptions))
        {
            authBuilder.AddFacebook(options =>
            {
                options.ClientId = facebookOptions.ClientId;
                options.ClientSecret = facebookOptions.ClientSecret;
                options.CallbackPath = "/signin-facebook";
                options.SignInScheme = "cookie";
                options.SaveTokens = true;
            });
        }

        if (identityOptions.ExternalProviders.TryGetValue("Twitter", out var twitterOptions))
        {
            authBuilder.AddTwitter(options =>
            {
                options.ClientId = twitterOptions.ClientId;
                options.ClientSecret = twitterOptions.ClientSecret;
                options.CallbackPath = "/signin-twitter";
                options.SignInScheme = "cookie";
                options.Scope.Add("users.read");
                options.Scope.Add("users.email");
                options.SaveTokens = true;
            });
        }

        if (identityOptions.ExternalProviders.TryGetValue("Steam", out var steamOptions))
        {
            authBuilder.AddSteam(options =>
            {
                options.ApplicationKey = steamOptions.ClientId;
                options.CallbackPath = "/signin-steam";
                options.SignInScheme = "cookie";
                options.SaveTokens = true;
            });
        }

        return builder;
    }
}
