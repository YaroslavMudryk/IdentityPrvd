using IdentityPrvd.Data.Queries;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Data.Transactions;
using IdentityPrvd.Infrastructure.Database.Context;
using IdentityPrvd.Infrastructure.Database.Transactions;
using IdentityPrvd.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Extensions;

/// <summary>
/// Builder for database services with support for different implementations
/// </summary>
public class DatabaseBuilder(IServiceCollection services, IdentityPrvdOptions options)
{
    private readonly IdentityPrvdOptions _options = options;

    /// <summary>
    /// Add Entity Framework context with PostgreSQL
    /// </summary>
    public DatabaseBuilder AddDbContext()
    {
        services.AddDbContext<IdentityPrvdContext>(options =>
            options.UseNpgsql(_options.Connections.Db)
                .UseSnakeCaseNamingConvention());
        return this;
    }

    /// <summary>
    /// Add transaction services with EF Core implementation
    /// </summary>
    public DatabaseBuilder AddTransaction<TTransactionManager>() where TTransactionManager : class, ITransactionManager
    {
        services.AddScoped<ITransactionManager, TTransactionManager>();
        return this;
    }

    public DatabaseBuilder AddTransaction()
    {
        AddTransaction<EfCoreTransactionManager>();
        return this;
    }

    /// <summary>
    /// Use custom queries implementation
    /// </summary>
    public DatabaseBuilder AddQueries<
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
        TUsersQuery>()
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
        services.AddScoped<IBansQuery, TBansQuery>();
        services.AddScoped<IClaimsQuery, TClaimsQuery>();
        services.AddScoped<IClientClaimsQuery, TClientClaimsQuery>();
        services.AddScoped<IClientSecretsQuery, TClientSecretsQuery>();
        services.AddScoped<IClientsQuery, TClientsQuery>();
        services.AddScoped<IConfirmsQuery, TConfirmsQuery>();
        services.AddScoped<IContactsQuery, TContactsQuery>();
        services.AddScoped<IDevicesQuery, TDevicesQuery>();
        services.AddScoped<IFailedLoginAttemptsQuery, TFailedLoginAttemptsQuery>();
        services.AddScoped<IMfaRecoveryCodesQuery, TMfaRecoveryCodesQuery>();
        services.AddScoped<IMfasQuery, TMfasQuery>();
        services.AddScoped<IPasswordsQuery, TPasswordsQuery>();
        services.AddScoped<IQrsQuery, TQrsQuery>();
        services.AddScoped<IRefreshTokensQuery, TRefreshTokensQuery>();
        services.AddScoped<IRoleClaimsQuery, TRoleClaimsQuery>();
        services.AddScoped<IRolesQuery, TRolesQuery>();
        services.AddScoped<ISessionsQuery, TSessionsQuery>();
        services.AddScoped<IUserLoginsQuery, TUserLoginsQuery>();
        services.AddScoped<IUserRolesQuery, TUserRolesQuery>();
        services.AddScoped<IUsersQuery, TUsersQuery>();
        return this;
    }

    /// <summary>
    /// Add database queries with EF Core implementation
    /// </summary>
    public DatabaseBuilder AddQueries()
    {
        AddQueries<EfBansQuery,
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
            EfUsersQuery>();
        return this;
    }

    /// <summary>
    /// Use custom stores implementation
    /// </summary>
    public DatabaseBuilder AddStores<
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
        TUserStore>()
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
        services.AddScoped<IBanStore, TBanStore>();
        services.AddScoped<IClaimStore, TClaimStore>();
        services.AddScoped<IClientClaimStore, TClientClaimStore>();
        services.AddScoped<IClientSecretStore, TClientSecretStore>();
        services.AddScoped<IClientStore, TClientStore>();
        services.AddScoped<IConfirmStore, TConfirmStore>();
        services.AddScoped<IContactStore, TContactStore>();
        services.AddScoped<IDeviceStore, TDeviceStore>();
        services.AddScoped<IFailedLoginAttemptStore, TFailedLoginAttemptStore>();
        services.AddScoped<IMfaRecoveryCodeStore, TMfaRecoveryCodeStore>();
        services.AddScoped<IMfaStore, TMfaStore>();
        services.AddScoped<IPasswordStore, TPasswordStore>();
        services.AddScoped<IQrStore, TQrStore>();
        services.AddScoped<IRefreshTokenStore, TRefreshTokenStore>();
        services.AddScoped<IRoleClaimStore, TRoleClaimStore>();
        services.AddScoped<IRoleStore, TRoleStore>();
        services.AddScoped<ISessionStore, TSessionStore>();
        services.AddScoped<IUserLoginStore, TUserLoginStore>();
        services.AddScoped<IUserRoleStore, TUserRoleStore>();
        services.AddScoped<IUserStore, TUserStore>();
        return this;
    }

    /// <summary>
    /// Add database stores with EF Core implementation
    /// </summary>
    public DatabaseBuilder AddStores()
    {
        AddStores<
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
            EfUserStore>();
        return this;
    }
}
