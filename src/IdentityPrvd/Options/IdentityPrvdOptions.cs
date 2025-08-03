using IdentityPrvd.Data.Queries;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Data.Transactions;
using IdentityPrvd.Infrastructure.Caching;
using IdentityPrvd.Infrastructure.Database.Context;
using IdentityPrvd.Infrastructure.Database.Transactions;
using IdentityPrvd.Services.Location;
using IdentityPrvd.Services.Notification;
using IdentityPrvd.Services.ServerSideSessions;
using Microsoft.Extensions.DependencyInjection;
using Redis.OM;
using Redis.OM.Contracts;

namespace IdentityPrvd.Options;

/// <summary>
/// Configuration options for IdentityPrvd
/// </summary>
public class IdentityPrvdOptions
{
    public IdentityPrvdOptions()
    {
        Connections = new IdentityConnectionOptions();
        ExternalProviders = new Dictionary<string, ExternalProviderOptions>();
        Notifiers = new NotifierConfiguration();
        Sessions = new SessionConfiguration();
        Database = new DatabaseConfiguration();
        Location = new LocationConfiguration();

        Token = new TokenOptions();
        User = new UserOptions();
        Language = new LanguageOptions();
        App = new AppOptions();
        Password = new PasswordOptions();
        Protection = new ProtectionOptions();
    }

    public IdentityConnectionOptions Connections { get; set; }
    public TokenOptions Token { get; set; }
    public Dictionary<string, ExternalProviderOptions> ExternalProviders { get; set; }
    public NotifierConfiguration Notifiers { get; set; }
    public SessionConfiguration Sessions { get; set; }
    public DatabaseConfiguration Database { get; set; }
    public LocationConfiguration Location { get; set; }
    public LanguageOptions Language { get; set; }
    public UserOptions User { get; set; }
    public AppOptions App { get; set; }
    public PasswordOptions Password { get; set; }
    public ProtectionOptions Protection { get; set; }
    public bool TrackSessionActivity { get; set; } = true;

    public void ValidateAndThrowIfNeeded()
    {
        // Validation logic here
    }
}

/// <summary>
/// Configuration for notification services
/// </summary>
public class NotifierConfiguration
{
    private readonly List<ServiceDescriptor> _emailServices = new();
    private readonly List<ServiceDescriptor> _smsServices = new();

    /// <summary>
    /// Add custom email service
    /// </summary>
    public NotifierConfiguration AddCustomEmailService<TEmailService>() where TEmailService : class, IEmailService
    {
        _emailServices.Add(new ServiceDescriptor(typeof(IEmailService), typeof(TEmailService), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Add custom SMS service
    /// </summary>
    public NotifierConfiguration AddCustomSmsService<TSmsService>() where TSmsService : class, ISmsService
    {
        _smsServices.Add(new ServiceDescriptor(typeof(ISmsService), typeof(TSmsService), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Add Twilio SMS service
    /// </summary>
    public NotifierConfiguration AddTwilioSms(string accountSid, string authToken, string fromNumber)
    {
        // This would be implemented when you create a Twilio SMS service
        // _smsServices.Add(new ServiceDescriptor(typeof(ISmsService), typeof(TwilioSmsService), ServiceLifetime.Scoped));
        // Configure Twilio options
        return this;
    }

    /// <summary>
    /// Add SendGrid email service
    /// </summary>
    public NotifierConfiguration AddSendGridEmail(string apiKey, string fromEmail, string fromName)
    {
        // This would be implemented when you create a SendGrid email service
        // _emailServices.Add(new ServiceDescriptor(typeof(IEmailService), typeof(SendGridEmailService), ServiceLifetime.Scoped));
        // Configure SendGrid options
        return this;
    }

    /// <summary>
    /// Add Azure email service
    /// </summary>
    public NotifierConfiguration AddAzureEmail(string connectionString, string fromEmail, string fromName)
    {
        // This would be implemented when you create an Azure email service
        // _emailServices.Add(new ServiceDescriptor(typeof(IEmailService), typeof(AzureEmailService), ServiceLifetime.Scoped));
        // Configure Azure options
        return this;
    }

    /// <summary>
    /// Use fake services (default for development)
    /// </summary>
    public NotifierConfiguration UseFakeServices()
    {
        _emailServices.Clear();
        _smsServices.Clear();
        _emailServices.Add(new ServiceDescriptor(typeof(IEmailService), typeof(FakeEmailService), ServiceLifetime.Scoped));
        _smsServices.Add(new ServiceDescriptor(typeof(ISmsService), typeof(FakeSmsService), ServiceLifetime.Scoped));
        return this;
    }

    internal void RegisterServices(IServiceCollection services)
    {
        // Register email services (use the last one added, or default to fake)
        if (_emailServices.Count > 0)
        {
            services.Add(_emailServices.Last());
        }
        else
        {
            services.AddScoped<IEmailService, FakeEmailService>();
        }

        // Register SMS services (use the last one added, or default to fake)
        if (_smsServices.Count > 0)
        {
            services.Add(_smsServices.Last());
        }
        else
        {
            services.AddScoped<ISmsService, FakeSmsService>();
        }
    }
}

/// <summary>
/// Configuration for session services
/// </summary>
public class SessionConfiguration
{
    private ServiceDescriptor? _sessionManager;
    private ServiceDescriptor? _sessionStore;

    /// <summary>
    /// Add custom session manager
    /// </summary>
    public SessionConfiguration AddCustomManager<TSessionManager>() where TSessionManager : class, ISessionManager
    {
        _sessionManager = new ServiceDescriptor(typeof(ISessionManager), typeof(TSessionManager), ServiceLifetime.Scoped);
        return this;
    }

    /// <summary>
    /// Add custom session store
    /// </summary>
    public SessionConfiguration AddCustomStore<TSessionStore>() where TSessionStore : class, Infrastructure.Caching.ISessionStore
    {
        _sessionStore = new ServiceDescriptor(typeof(Infrastructure.Caching.ISessionStore), typeof(TSessionStore), ServiceLifetime.Scoped);
        return this;
    }

    /// <summary>
    /// Use SQL Server session manager
    /// </summary>
    public SessionConfiguration UseSqlServerManager(string connectionString)
    {
        // This would be implemented when you create a SQL Server session manager
        // _sessionManager = new ServiceDescriptor(typeof(ISessionManager), typeof(SqlServerSessionManager), ServiceLifetime.Scoped);
        // Configure SQL Server options
        return this;
    }

    /// <summary>
    /// Use Redis sessions (default)
    /// </summary>
    public SessionConfiguration UseRedisSessions()
    {
        _sessionManager = new ServiceDescriptor(typeof(ISessionManager), typeof(SessionManager), ServiceLifetime.Scoped);
        _sessionStore = new ServiceDescriptor(typeof(Infrastructure.Caching.ISessionStore), typeof(RedisSessionStore), ServiceLifetime.Scoped);
        return this;
    }

    /// <summary>
    /// Use in-memory sessions (for development)
    /// </summary>
    public SessionConfiguration UseInMemorySessions()
    {
        _sessionManager = new ServiceDescriptor(typeof(ISessionManager), typeof(SessionManager), ServiceLifetime.Scoped);
        _sessionStore = new ServiceDescriptor(typeof(Infrastructure.Caching.ISessionStore), typeof(InMemorySessionStore), ServiceLifetime.Scoped);
        return this;
    }

    internal void RegisterServices(IServiceCollection services)
    {
        // Register session manager (use custom or default)
        if (_sessionManager != null)
        {
            services.Add(_sessionManager);
        }
        else
        {
            services.AddScoped<ISessionManager, SessionManager>();
        }

        // Register session store (use custom or default to Redis)
        if (_sessionStore != null)
        {
            services.Add(_sessionStore);
        }
        else
        {
            services.AddScoped<Infrastructure.Caching.ISessionStore, RedisSessionStore>();
        }

        // Always register middleware
        services.AddTransient<ServerSideSessionMiddleware>();
    }
}

/// <summary>
/// Configuration for database services
/// </summary>
public class DatabaseConfiguration
{
    private readonly List<ServiceDescriptor> _queries = new();
    private readonly List<ServiceDescriptor> _stores = new();
    private readonly HashSet<Type> _skippedQueries = new();
    private readonly HashSet<Type> _skippedStores = new();
    private ServiceDescriptor? _transactionManager;
    private ServiceDescriptor? _redisConnection;
    private string? _connectionString;
    private string? _databaseType;

    /// <summary>
    /// Add PostgreSQL database
    /// </summary>
    public DatabaseConfiguration AddPostgres(string connectionString)
    {
        _connectionString = connectionString;
        _databaseType = "PostgreSQL";
        return this;
    }

    /// <summary>
    /// Add SQL Server database
    /// </summary>
    public DatabaseConfiguration AddSqlServer(string connectionString)
    {
        _connectionString = connectionString;
        _databaseType = "SQLServer";
        return this;
    }

    /// <summary>
    /// Add MySQL database
    /// </summary>
    public DatabaseConfiguration AddMySql(string connectionString)
    {
        _connectionString = connectionString;
        _databaseType = "MySQL";
        return this;
    }

    /// <summary>
    /// Add custom clients query
    /// </summary>
    public DatabaseConfiguration AddClientsQuery<TClientsQuery>() where TClientsQuery : class, IClientsQuery
    {
        _queries.Add(new ServiceDescriptor(typeof(IClientsQuery), typeof(TClientsQuery), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Add custom clients store
    /// </summary>
    public DatabaseConfiguration AddClientStore<TClientsStore>() where TClientsStore : class, IClientStore
    {
        // Note: IClientsStore interface doesn't exist yet, this is a placeholder
        // _stores.Add(new ServiceDescriptor(typeof(IClientsStore), typeof(TClientsStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Add custom users query
    /// </summary>
    public DatabaseConfiguration AddUsersQuery<TUsersQuery>() where TUsersQuery : class, IUsersQuery
    {
        _queries.Add(new ServiceDescriptor(typeof(IUsersQuery), typeof(TUsersQuery), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Add custom users store
    /// </summary>
    public DatabaseConfiguration AddUserStore<TUserStore>() where TUserStore : class, IUserStore
    {
        _stores.Add(new ServiceDescriptor(typeof(IUserStore), typeof(TUserStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Add custom claims query
    /// </summary>
    public DatabaseConfiguration AddClaimsQuery<TClaimsQuery>() where TClaimsQuery : class, IClaimsQuery
    {
        _queries.Add(new ServiceDescriptor(typeof(IClaimsQuery), typeof(TClaimsQuery), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Add custom claims store
    /// </summary>
    public DatabaseConfiguration AddClaimStore<TClaimStore>() where TClaimStore : class, IClaimStore
    {
        _stores.Add(new ServiceDescriptor(typeof(IClaimStore), typeof(TClaimStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Add custom roles query
    /// </summary>
    public DatabaseConfiguration AddRolesQuery<TRolesQuery>() where TRolesQuery : class, IRolesQuery
    {
        _queries.Add(new ServiceDescriptor(typeof(IRolesQuery), typeof(TRolesQuery), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Add custom roles store
    /// </summary>
    public DatabaseConfiguration AddRoleStore<TRoleStore>() where TRoleStore : class, IRoleStore
    {
        _stores.Add(new ServiceDescriptor(typeof(IRoleStore), typeof(TRoleStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Add custom refresh tokens query
    /// </summary>
    public DatabaseConfiguration AddRefreshTokensQuery<TRefreshTokensQuery>() where TRefreshTokensQuery : class, IRefreshTokensQuery
    {
        _queries.Add(new ServiceDescriptor(typeof(IRefreshTokensQuery), typeof(TRefreshTokensQuery), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Add custom refresh tokens store
    /// </summary>
    public DatabaseConfiguration AddRefreshTokenStore<TRefreshTokenStore>() where TRefreshTokenStore : class, IRefreshTokenStore
    {
        _stores.Add(new ServiceDescriptor(typeof(IRefreshTokenStore), typeof(TRefreshTokenStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Add custom sessions query
    /// </summary>
    public DatabaseConfiguration AddSessionsQuery<TSessionsQuery>() where TSessionsQuery : class, ISessionsQuery
    {
        _queries.Add(new ServiceDescriptor(typeof(ISessionsQuery), typeof(TSessionsQuery), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Add custom sessions store
    /// </summary>
    public DatabaseConfiguration AddSessionStore<TSessionStore>() where TSessionStore : class, Data.Stores.ISessionStore
    {
        _stores.Add(new ServiceDescriptor(typeof(Data.Stores.ISessionStore), typeof(TSessionStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Add custom user logins query
    /// </summary>
    public DatabaseConfiguration AddUserLoginsQuery<TUserLoginsQuery>() where TUserLoginsQuery : class, IUserLoginsQuery
    {
        _queries.Add(new ServiceDescriptor(typeof(IUserLoginsQuery), typeof(TUserLoginsQuery), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Add custom user logins store
    /// </summary>
    public DatabaseConfiguration AddUserLoginStore<TUserLoginStore>() where TUserLoginStore : class, IUserLoginStore
    {
        _stores.Add(new ServiceDescriptor(typeof(IUserLoginStore), typeof(TUserLoginStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Add custom user roles query
    /// </summary>
    public DatabaseConfiguration AddUserRolesQuery<TUserRolesQuery>() where TUserRolesQuery : class, IUserRolesQuery
    {
        _queries.Add(new ServiceDescriptor(typeof(IUserRolesQuery), typeof(TUserRolesQuery), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Add custom user roles store
    /// </summary>
    public DatabaseConfiguration AddUserRoleStore<TUserRoleStore>() where TUserRoleStore : class, IUserRoleStore
    {
        _stores.Add(new ServiceDescriptor(typeof(IUserRoleStore), typeof(TUserRoleStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Add custom contacts query
    /// </summary>
    public DatabaseConfiguration AddContactsQuery<TContactsQuery>() where TContactsQuery : class, IContactsQuery
    {
        _queries.Add(new ServiceDescriptor(typeof(IContactsQuery), typeof(TContactsQuery), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Add custom contacts store
    /// </summary>
    public DatabaseConfiguration AddContactStore<TContactStore>() where TContactStore : class, IContactStore
    {
        _stores.Add(new ServiceDescriptor(typeof(IContactStore), typeof(TContactStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Add custom devices query
    /// </summary>
    public DatabaseConfiguration AddDevicesQuery<TDevicesQuery>() where TDevicesQuery : class, IDevicesQuery
    {
        _queries.Add(new ServiceDescriptor(typeof(IDevicesQuery), typeof(TDevicesQuery), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Add custom devices store
    /// </summary>
    public DatabaseConfiguration AddDeviceStore<TDeviceStore>() where TDeviceStore : class, IDeviceStore
    {
        _stores.Add(new ServiceDescriptor(typeof(IDeviceStore), typeof(TDeviceStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Skip claims query registration
    /// </summary>
    public DatabaseConfiguration SkipClaimsQuery()
    {
        _skippedQueries.Add(typeof(IClaimsQuery));
        return this;
    }

    /// <summary>
    /// Skip clients query registration
    /// </summary>
    public DatabaseConfiguration SkipClientsQuery()
    {
        _skippedQueries.Add(typeof(IClientsQuery));
        return this;
    }

    /// <summary>
    /// Skip refresh tokens query registration
    /// </summary>
    public DatabaseConfiguration SkipRefreshTokensQuery()
    {
        _skippedQueries.Add(typeof(IRefreshTokensQuery));
        return this;
    }

    /// <summary>
    /// Skip roles query registration
    /// </summary>
    public DatabaseConfiguration SkipRolesQuery()
    {
        _skippedQueries.Add(typeof(IRolesQuery));
        return this;
    }

    /// <summary>
    /// Skip sessions query registration
    /// </summary>
    public DatabaseConfiguration SkipSessionsQuery()
    {
        _skippedQueries.Add(typeof(ISessionsQuery));
        return this;
    }

    /// <summary>
    /// Skip user logins query registration
    /// </summary>
    public DatabaseConfiguration SkipUserLoginsQuery()
    {
        _skippedQueries.Add(typeof(IUserLoginsQuery));
        return this;
    }

    /// <summary>
    /// Skip users query registration
    /// </summary>
    public DatabaseConfiguration SkipUsersQuery()
    {
        _skippedQueries.Add(typeof(IUsersQuery));
        return this;
    }

    /// <summary>
    /// Skip contacts query registration
    /// </summary>
    public DatabaseConfiguration SkipContactsQuery()
    {
        _skippedQueries.Add(typeof(IContactsQuery));
        return this;
    }

    /// <summary>
    /// Skip devices query registration
    /// </summary>
    public DatabaseConfiguration SkipDevicesQuery()
    {
        _skippedQueries.Add(typeof(IDevicesQuery));
        return this;
    }

    /// <summary>
    /// Skip user roles query registration
    /// </summary>
    public DatabaseConfiguration SkipUserRolesQuery()
    {
        _skippedQueries.Add(typeof(IUserRolesQuery));
        return this;
    }

    /// <summary>
    /// Skip claim store registration
    /// </summary>
    public DatabaseConfiguration SkipClaimStore()
    {
        _skippedStores.Add(typeof(IClaimStore));
        return this;
    }

    /// <summary>
    /// Skip confirm store registration
    /// </summary>
    public DatabaseConfiguration SkipConfirmStore()
    {
        _skippedStores.Add(typeof(IConfirmStore));
        return this;
    }

    /// <summary>
    /// Skip MFA store registration
    /// </summary>
    public DatabaseConfiguration SkipMfaStore()
    {
        _skippedStores.Add(typeof(IMfaStore));
        return this;
    }

    /// <summary>
    /// Skip password store registration
    /// </summary>
    public DatabaseConfiguration SkipPasswordStore()
    {
        _skippedStores.Add(typeof(IPasswordStore));
        return this;
    }

    /// <summary>
    /// Skip refresh token store registration
    /// </summary>
    public DatabaseConfiguration SkipRefreshTokenStore()
    {
        _skippedStores.Add(typeof(IRefreshTokenStore));
        return this;
    }

    /// <summary>
    /// Skip role claim store registration
    /// </summary>
    public DatabaseConfiguration SkipRoleClaimStore()
    {
        _skippedStores.Add(typeof(IRoleClaimStore));
        return this;
    }

    /// <summary>
    /// Skip role store registration
    /// </summary>
    public DatabaseConfiguration SkipRoleStore()
    {
        _skippedStores.Add(typeof(IRoleStore));
        return this;
    }

    /// <summary>
    /// Skip session store registration
    /// </summary>
    public DatabaseConfiguration SkipSessionStore()
    {
        _skippedStores.Add(typeof(Data.Stores.ISessionStore));
        return this;
    }

    /// <summary>
    /// Skip user login store registration
    /// </summary>
    public DatabaseConfiguration SkipUserLoginStore()
    {
        _skippedStores.Add(typeof(IUserLoginStore));
        return this;
    }

    /// <summary>
    /// Skip user role store registration
    /// </summary>
    public DatabaseConfiguration SkipUserRoleStore()
    {
        _skippedStores.Add(typeof(IUserRoleStore));
        return this;
    }

    /// <summary>
    /// Skip user store registration
    /// </summary>
    public DatabaseConfiguration SkipUserStore()
    {
        _skippedStores.Add(typeof(IUserStore));
        return this;
    }

    /// <summary>
    /// Skip contact store registration
    /// </summary>
    public DatabaseConfiguration SkipContactStore()
    {
        _skippedStores.Add(typeof(IContactStore));
        return this;
    }

    /// <summary>
    /// Skip device store registration
    /// </summary>
    public DatabaseConfiguration SkipDeviceStore()
    {
        _skippedStores.Add(typeof(IDeviceStore));
        return this;
    }

    /// <summary>
    /// Skip QR store registration
    /// </summary>
    public DatabaseConfiguration SkipQrStore()
    {
        _skippedStores.Add(typeof(IQrStore));
        return this;
    }

    /// <summary>
    /// Skip MFA recovery code store registration
    /// </summary>
    public DatabaseConfiguration SkipMfaRecoveryCodeStore()
    {
        _skippedStores.Add(typeof(IMfaRecoveryCodeStore));
        return this;
    }

    /// <summary>
    /// Skip failed login attempt store registration
    /// </summary>
    public DatabaseConfiguration SkipFailedLoginAttemptStore()
    {
        _skippedStores.Add(typeof(IFailedLoginAttemptStore));
        return this;
    }

    /// <summary>
    /// Skip client store registration
    /// </summary>
    public DatabaseConfiguration SkipClientStore()
    {
        _skippedStores.Add(typeof(IClientStore));
        return this;
    }

    /// <summary>
    /// Skip client secret store registration
    /// </summary>
    public DatabaseConfiguration SkipClientSecretStore()
    {
        _skippedStores.Add(typeof(IClientSecretStore));
        return this;
    }

    /// <summary>
    /// Skip client claim store registration
    /// </summary>
    public DatabaseConfiguration SkipClientClaimStore()
    {
        _skippedStores.Add(typeof(IClientClaimStore));
        return this;
    }

    /// <summary>
    /// Skip ban store registration
    /// </summary>
    public DatabaseConfiguration SkipBanStore()
    {
        _skippedStores.Add(typeof(IBanStore));
        return this;
    }

    /// <summary>
    /// Skip all query registrations
    /// </summary>
    public DatabaseConfiguration SkipAllQueries()
    {
        _skippedQueries.Add(typeof(IClaimsQuery));
        _skippedQueries.Add(typeof(IClientsQuery));
        _skippedQueries.Add(typeof(IRefreshTokensQuery));
        _skippedQueries.Add(typeof(IRolesQuery));
        _skippedQueries.Add(typeof(ISessionsQuery));
        _skippedQueries.Add(typeof(IUserLoginsQuery));
        _skippedQueries.Add(typeof(IUsersQuery));
        _skippedQueries.Add(typeof(IContactsQuery));
        _skippedQueries.Add(typeof(IDevicesQuery));
        _skippedQueries.Add(typeof(IUserRolesQuery));
        _skippedQueries.Add(typeof(IBansQuery));
        _skippedQueries.Add(typeof(IPasswordsQuery));
        _skippedQueries.Add(typeof(IRoleClaimsQuery));
        _skippedQueries.Add(typeof(IQrsQuery));
        _skippedQueries.Add(typeof(IMfaRecoveryCodesQuery));
        _skippedQueries.Add(typeof(IFailedLoginAttemptsQuery));
        _skippedQueries.Add(typeof(IMfasQuery));
        _skippedQueries.Add(typeof(IConfirmsQuery));
        _skippedQueries.Add(typeof(IClientSecretsQuery));
        _skippedQueries.Add(typeof(IClientClaimsQuery));
        return this;
    }

    /// <summary>
    /// Skip all store registrations
    /// </summary>
    public DatabaseConfiguration SkipAllStores()
    {
        _skippedStores.Add(typeof(IClaimStore));
        _skippedStores.Add(typeof(IConfirmStore));
        _skippedStores.Add(typeof(IMfaStore));
        _skippedStores.Add(typeof(IPasswordStore));
        _skippedStores.Add(typeof(IRefreshTokenStore));
        _skippedStores.Add(typeof(IRoleClaimStore));
        _skippedStores.Add(typeof(IRoleStore));
        _skippedStores.Add(typeof(Data.Stores.ISessionStore));
        _skippedStores.Add(typeof(IUserLoginStore));
        _skippedStores.Add(typeof(IUserRoleStore));
        _skippedStores.Add(typeof(IUserStore));
        _skippedStores.Add(typeof(IContactStore));
        _skippedStores.Add(typeof(IDeviceStore));
        _skippedStores.Add(typeof(IQrStore));
        _skippedStores.Add(typeof(IMfaRecoveryCodeStore));
        _skippedStores.Add(typeof(IFailedLoginAttemptStore));
        _skippedStores.Add(typeof(IClientStore));
        _skippedStores.Add(typeof(IClientSecretStore));
        _skippedStores.Add(typeof(IClientClaimStore));
        _skippedStores.Add(typeof(IBanStore));
        return this;
    }

    /// <summary>
    /// Skip all user-related queries
    /// </summary>
    public DatabaseConfiguration SkipUserQueries()
    {
        _skippedQueries.Add(typeof(IUsersQuery));
        _skippedQueries.Add(typeof(IUserLoginsQuery));
        _skippedQueries.Add(typeof(IUserRolesQuery));
        return this;
    }

    /// <summary>
    /// Skip all user-related stores
    /// </summary>
    public DatabaseConfiguration SkipUserStores()
    {
        _skippedStores.Add(typeof(IUserStore));
        _skippedStores.Add(typeof(IUserLoginStore));
        _skippedStores.Add(typeof(IUserRoleStore));
        return this;
    }

    /// <summary>
    /// Skip all role-related queries and stores
    /// </summary>
    public DatabaseConfiguration SkipRoleServices()
    {
        _skippedQueries.Add(typeof(IRolesQuery));
        _skippedStores.Add(typeof(IRoleStore));
        _skippedStores.Add(typeof(IRoleClaimStore));
        return this;
    }

    /// <summary>
    /// Skip all claim-related queries and stores
    /// </summary>
    public DatabaseConfiguration SkipClaimServices()
    {
        _skippedQueries.Add(typeof(IClaimsQuery));
        _skippedStores.Add(typeof(IClaimStore));
        return this;
    }

    /// <summary>
    /// Skip all client-related queries and stores
    /// </summary>
    public DatabaseConfiguration SkipClientServices()
    {
        _skippedQueries.Add(typeof(IClientsQuery));
        _skippedQueries.Add(typeof(IClientSecretsQuery));
        _skippedQueries.Add(typeof(IClientClaimsQuery));
        _skippedStores.Add(typeof(IClientStore));
        _skippedStores.Add(typeof(IClientSecretStore));
        _skippedStores.Add(typeof(IClientClaimStore));
        return this;
    }

    /// <summary>
    /// Skip all MFA-related queries and stores
    /// </summary>
    public DatabaseConfiguration SkipMfaServices()
    {
        _skippedQueries.Add(typeof(IMfasQuery));
        _skippedQueries.Add(typeof(IMfaRecoveryCodesQuery));
        _skippedStores.Add(typeof(IMfaStore));
        _skippedStores.Add(typeof(IMfaRecoveryCodeStore));
        return this;
    }

    /// <summary>
    /// Skip all security-related queries and stores
    /// </summary>
    public DatabaseConfiguration SkipSecurityServices()
    {
        _skippedQueries.Add(typeof(IBansQuery));
        _skippedQueries.Add(typeof(IFailedLoginAttemptsQuery));
        _skippedStores.Add(typeof(IBanStore));
        _skippedStores.Add(typeof(IFailedLoginAttemptStore));
        return this;
    }

    /// <summary>
    /// Skip all QR-related queries and stores
    /// </summary>
    public DatabaseConfiguration SkipQrServices()
    {
        _skippedQueries.Add(typeof(IQrsQuery));
        _skippedStores.Add(typeof(IQrStore));
        return this;
    }

    /// <summary>
    /// Skip bans query registration
    /// </summary>
    public DatabaseConfiguration SkipBansQuery()
    {
        _skippedQueries.Add(typeof(IBansQuery));
        return this;
    }

    /// <summary>
    /// Skip passwords query registration
    /// </summary>
    public DatabaseConfiguration SkipPasswordsQuery()
    {
        _skippedQueries.Add(typeof(IPasswordsQuery));
        return this;
    }

    /// <summary>
    /// Skip role claims query registration
    /// </summary>
    public DatabaseConfiguration SkipRoleClaimsQuery()
    {
        _skippedQueries.Add(typeof(IRoleClaimsQuery));
        return this;
    }

    /// <summary>
    /// Skip QR query registration
    /// </summary>
    public DatabaseConfiguration SkipQrsQuery()
    {
        _skippedQueries.Add(typeof(IQrsQuery));
        return this;
    }

    /// <summary>
    /// Skip MFA recovery codes query registration
    /// </summary>
    public DatabaseConfiguration SkipMfaRecoveryCodesQuery()
    {
        _skippedQueries.Add(typeof(IMfaRecoveryCodesQuery));
        return this;
    }

    /// <summary>
    /// Skip failed login attempts query registration
    /// </summary>
    public DatabaseConfiguration SkipFailedLoginAttemptsQuery()
    {
        _skippedQueries.Add(typeof(IFailedLoginAttemptsQuery));
        return this;
    }

    /// <summary>
    /// Skip MFAs query registration
    /// </summary>
    public DatabaseConfiguration SkipMfasQuery()
    {
        _skippedQueries.Add(typeof(IMfasQuery));
        return this;
    }

    /// <summary>
    /// Skip confirms query registration
    /// </summary>
    public DatabaseConfiguration SkipConfirmsQuery()
    {
        _skippedQueries.Add(typeof(IConfirmsQuery));
        return this;
    }

    /// <summary>
    /// Skip client secrets query registration
    /// </summary>
    public DatabaseConfiguration SkipClientSecretsQuery()
    {
        _skippedQueries.Add(typeof(IClientSecretsQuery));
        return this;
    }

    /// <summary>
    /// Skip client claims query registration
    /// </summary>
    public DatabaseConfiguration SkipClientClaimsQuery()
    {
        _skippedQueries.Add(typeof(IClientClaimsQuery));
        return this;
    }

    /// <summary>
    /// Add custom transaction manager
    /// </summary>
    public DatabaseConfiguration AddTransactionManager<TTransactionManager>() where TTransactionManager : class, ITransactionManager
    {
        _transactionManager = new ServiceDescriptor(typeof(ITransactionManager), typeof(TTransactionManager), ServiceLifetime.Scoped);
        return this;
    }

    /// <summary>
    /// Add custom Redis connection
    /// </summary>
    public DatabaseConfiguration AddRedisConnection<TRedisConnection>() where TRedisConnection : class, IRedisConnectionProvider
    {
        _redisConnection = new ServiceDescriptor(typeof(IRedisConnectionProvider), typeof(TRedisConnection), ServiceLifetime.Scoped);
        return this;
    }

    /// <summary>
    /// Add custom database provider strategy
    /// </summary>
    public DatabaseConfiguration AddCustomProviderStrategy<TStrategy>() where TStrategy : class, IDatabaseProviderStrategy
    {
        // This will be handled by the DatabaseProviderManager
        return this;
    }

    /// <summary>
    /// Use EF Core (default)
    /// </summary>
    public DatabaseConfiguration UseEfCore()
    {
        // Clear custom queries and stores to use EF Core defaults
        _queries.Clear();
        _stores.Clear();
        _transactionManager = null;
        return this;
    }

    /// <summary>
    /// Use Dapper
    /// </summary>
    public DatabaseConfiguration UseDapper()
    {
        // This would be implemented to add all Dapper implementations
        // For now, just clear to indicate Dapper should be used
        _queries.Clear();
        _stores.Clear();
        _transactionManager = null;
        return this;
    }

    internal void RegisterServices(IServiceCollection services, IdentityConnectionOptions connectionOptions)
    {
        // Register database provider manager
        services.AddDatabaseProviderManager();

        // Register database context based on type
        if (!string.IsNullOrEmpty(_connectionString))
        {
            switch (_databaseType?.ToLowerInvariant())
            {
                case "postgresql":
                    services.AddDbContext<IdentityPrvdContext>((serviceProvider, options) =>
                    {
                        var providerManager = serviceProvider.GetRequiredService<DatabaseProviderManager>();
                        providerManager.ConfigureDbContext(options, "PostgreSQL", _connectionString);
                    });
                    connectionOptions.Db = _connectionString;
                    break;
                case "sqlserver":
                    services.AddDbContext<IdentityPrvdContext>((serviceProvider, options) =>
                    {
                        var providerManager = serviceProvider.GetRequiredService<DatabaseProviderManager>();
                        providerManager.ConfigureDbContext(options, "SQLServer", _connectionString);
                    });
                    connectionOptions.Db = _connectionString;
                    break;
                case "mysql":
                    services.AddDbContext<IdentityPrvdContext>((serviceProvider, options) =>
                    {
                        var providerManager = serviceProvider.GetRequiredService<DatabaseProviderManager>();
                        providerManager.ConfigureDbContext(options, "MySQL", _connectionString);
                    });
                    connectionOptions.Db = _connectionString;
                    break;
                default:
                    // Use existing connection string from options
                    break;
            }
        }

        // Register custom queries
        foreach (var query in _queries)
        {
            services.Add(query);
        }

        // Register custom stores
        foreach (var store in _stores)
        {
            services.Add(store);
        }

        // Register transaction manager
        if (_transactionManager != null)
        {
            services.Add(_transactionManager);
        }
        else
        {
            services.AddScoped<ITransactionManager, EfCoreTransactionManager>();
        }

        // Register Redis connection
        if (_redisConnection != null)
        {
            services.Add(_redisConnection);
        }
        else
        {
            services.AddScoped<IRedisConnectionProvider>(_ =>
                new RedisConnectionProvider(connectionOptions.Redis));
        }

        // Register default EF Core services if no custom ones were added
        if (_queries.Count == 0)
        {
            RegisterDefaultQueries(services);
        }

        if (_stores.Count == 0)
        {
            RegisterDefaultStores(services);
        }
    }

    private void RegisterDefaultQueries(IServiceCollection services)
    {
        if (!_skippedQueries.Contains(typeof(IClaimsQuery)))
            services.AddScoped<IClaimsQuery, EfClaimsQuery>();

        if (!_skippedQueries.Contains(typeof(IClientsQuery)))
            services.AddScoped<IClientsQuery, EfClientsQuery>();

        if (!_skippedQueries.Contains(typeof(IRefreshTokensQuery)))
            services.AddScoped<IRefreshTokensQuery, EfRefreshTokensQuery>();

        if (!_skippedQueries.Contains(typeof(IRolesQuery)))
            services.AddScoped<IRolesQuery, EfRolesQuery>();

        if (!_skippedQueries.Contains(typeof(ISessionsQuery)))
            services.AddScoped<ISessionsQuery, EfSessionsQuery>();

        if (!_skippedQueries.Contains(typeof(IUserLoginsQuery)))
            services.AddScoped<IUserLoginsQuery, EfUserLoginsQuery>();

        if (!_skippedQueries.Contains(typeof(IUsersQuery)))
            services.AddScoped<IUsersQuery, EfUsersQuery>();

        if (!_skippedQueries.Contains(typeof(IContactsQuery)))
            services.AddScoped<IContactsQuery, EfContactsQuery>();

        if (!_skippedQueries.Contains(typeof(IDevicesQuery)))
            services.AddScoped<IDevicesQuery, EfDevicesQuery>();

        if (!_skippedQueries.Contains(typeof(IUserRolesQuery)))
            services.AddScoped<IUserRolesQuery, EfUserRolesQuery>();

        if (!_skippedQueries.Contains(typeof(IBansQuery)))
            services.AddScoped<IBansQuery, EfBansQuery>();

        if (!_skippedQueries.Contains(typeof(IPasswordsQuery)))
            services.AddScoped<IPasswordsQuery, EfPasswordsQuery>();

        if (!_skippedQueries.Contains(typeof(IRoleClaimsQuery)))
            services.AddScoped<IRoleClaimsQuery, EfRoleClaimsQuery>();

        if (!_skippedQueries.Contains(typeof(IQrsQuery)))
            services.AddScoped<IQrsQuery, EfQrsQuery>();

        if (!_skippedQueries.Contains(typeof(IMfaRecoveryCodesQuery)))
            services.AddScoped<IMfaRecoveryCodesQuery, EfMfaRecoveryCodesQuery>();

        if (!_skippedQueries.Contains(typeof(IFailedLoginAttemptsQuery)))
            services.AddScoped<IFailedLoginAttemptsQuery, EfFailedLoginAttemptsQuery>();

        if (!_skippedQueries.Contains(typeof(IMfasQuery)))
            services.AddScoped<IMfasQuery, EfMfasQuery>();

        if (!_skippedQueries.Contains(typeof(IConfirmsQuery)))
            services.AddScoped<IConfirmsQuery, EfConfirmsQuery>();

        if (!_skippedQueries.Contains(typeof(IClientSecretsQuery)))
            services.AddScoped<IClientSecretsQuery, EfClientSecretsQuery>();

        if (!_skippedQueries.Contains(typeof(IClientClaimsQuery)))
            services.AddScoped<IClientClaimsQuery, EfClientClaimsQuery>();
    }

    private void RegisterDefaultStores(IServiceCollection services)
    {
        if (!_skippedStores.Contains(typeof(IClaimStore)))
            services.AddScoped<IClaimStore, EfClaimStore>();

        if (!_skippedStores.Contains(typeof(IConfirmStore)))
            services.AddScoped<IConfirmStore, EfConfirmStore>();

        if (!_skippedStores.Contains(typeof(IMfaStore)))
            services.AddScoped<IMfaStore, EfMfaStore>();

        if (!_skippedStores.Contains(typeof(IPasswordStore)))
            services.AddScoped<IPasswordStore, EfPasswordStore>();

        if (!_skippedStores.Contains(typeof(IRefreshTokenStore)))
            services.AddScoped<IRefreshTokenStore, EfRefreshTokenStore>();

        if (!_skippedStores.Contains(typeof(IRoleClaimStore)))
            services.AddScoped<IRoleClaimStore, EfRoleClaimStore>();

        if (!_skippedStores.Contains(typeof(IRoleStore)))
            services.AddScoped<IRoleStore, EfRoleStore>();

        if (!_skippedStores.Contains(typeof(Data.Stores.ISessionStore)))
            services.AddScoped<Data.Stores.ISessionStore, EfSessionStore>();

        if (!_skippedStores.Contains(typeof(IUserLoginStore)))
            services.AddScoped<IUserLoginStore, EfUserLoginStore>();

        if (!_skippedStores.Contains(typeof(IUserRoleStore)))
            services.AddScoped<IUserRoleStore, EfUserRoleStore>();

        if (!_skippedStores.Contains(typeof(IUserStore)))
            services.AddScoped<IUserStore, EfUserStore>();

        if (!_skippedStores.Contains(typeof(IContactStore)))
            services.AddScoped<IContactStore, EfContactStore>();

        if (!_skippedStores.Contains(typeof(IDeviceStore)))
            services.AddScoped<IDeviceStore, EfDeviceStore>();

        if (!_skippedStores.Contains(typeof(IQrStore)))
            services.AddScoped<IQrStore, EfQrStore>();

        if (!_skippedStores.Contains(typeof(IMfaRecoveryCodeStore)))
            services.AddScoped<IMfaRecoveryCodeStore, EfMfaRecoveryCodeStore>();

        if (!_skippedStores.Contains(typeof(IFailedLoginAttemptStore)))
            services.AddScoped<IFailedLoginAttemptStore, EfFailedLoginAttemptStore>();

        if (!_skippedStores.Contains(typeof(IClientStore)))
            services.AddScoped<IClientStore, EfClientStore>();

        if (!_skippedStores.Contains(typeof(IClientSecretStore)))
            services.AddScoped<IClientSecretStore, EfClientSecretStore>();

        if (!_skippedStores.Contains(typeof(IClientClaimStore)))
            services.AddScoped<IClientClaimStore, EfClientClaimStore>();

        if (!_skippedStores.Contains(typeof(IBanStore)))
            services.AddScoped<IBanStore, EfBanStore>();
    }
}

/// <summary>
/// Configuration for location services
/// </summary>
public class LocationConfiguration
{
    private ServiceDescriptor? _locationService;

    /// <summary>
    /// Add custom location service
    /// </summary>
    public LocationConfiguration AddCustomLocationService<TLocationService>() where TLocationService : class, ILocationService
    {
        _locationService = new ServiceDescriptor(typeof(ILocationService), typeof(TLocationService), ServiceLifetime.Scoped);
        return this;
    }

    /// <summary>
    /// Use IP-API location service (default)
    /// </summary>
    public LocationConfiguration UseIpApiService()
    {
        _locationService = new ServiceDescriptor(typeof(ILocationService), typeof(IpApiLocationService), ServiceLifetime.Scoped);
        return this;
    }

    /// <summary>
    /// Use fake location service for testing
    /// </summary>
    public LocationConfiguration UseFakeService()
    {
        _locationService = new ServiceDescriptor(typeof(ILocationService), typeof(FakeLocationService), ServiceLifetime.Scoped);
        return this;
    }

    /// <summary>
    /// Use MaxMind GeoIP2 location service
    /// </summary>
    public LocationConfiguration UseMaxMindService(string licenseKey, string? databasePath = null)
    {
        // This would be implemented when you create a MaxMind location service
        // _locationService = new ServiceDescriptor(typeof(ILocationService), typeof(MaxMindLocationService), ServiceLifetime.Scoped);
        // Configure MaxMind options
        return this;
    }

    /// <summary>
    /// Use IP2Location service
    /// </summary>
    public LocationConfiguration UseIp2LocationService(string apiKey)
    {
        // This would be implemented when you create an IP2Location service
        // _locationService = new ServiceDescriptor(typeof(ILocationService), typeof(Ip2LocationService), ServiceLifetime.Scoped);
        // Configure IP2Location options
        return this;
    }

    /// <summary>
    /// Use IPGeolocation.io service
    /// </summary>
    public LocationConfiguration UseIpGeolocationService(string apiKey)
    {
        // This would be implemented when you create an IPGeolocation service
        // _locationService = new ServiceDescriptor(typeof(ILocationService), typeof(IpGeolocationService), ServiceLifetime.Scoped);
        // Configure IPGeolocation options
        return this;
    }

    internal void RegisterServices(IServiceCollection services)
    {
        if (_locationService != null)
        {
            services.Add(_locationService);

            // If using IpApiLocationService, also register the HttpClient
            if (_locationService.ImplementationType == typeof(IpApiLocationService))
            {
                services.AddHttpClient<ILocationService, IpApiLocationService>("Location", options =>
                {
                    options.BaseAddress = new Uri("http://ip-api.com");
                });
            }
        }
        else
        {
            // Default to IpApiLocationService
            services.AddHttpClient<ILocationService, IpApiLocationService>("Location", options =>
            {
                options.BaseAddress = new Uri("http://ip-api.com");
            });
        }
    }
}

/// <summary>
/// Extension methods for configuring external providers
/// </summary>
public static class ExternalProviderConfigurationExtensions
{
    /// <summary>
    /// Add external providers configuration
    /// </summary>
    public static ExternalProvidersConfiguration AddExternalProviders(this IdentityPrvdOptions options)
    {
        return new ExternalProvidersConfiguration(options);
    }
}

/// <summary>
/// Configuration for external providers
/// </summary>
public class ExternalProvidersConfiguration
{
    private readonly IdentityPrvdOptions _options;

    public ExternalProvidersConfiguration(IdentityPrvdOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// Add Google provider
    /// </summary>
    public ExternalProvidersConfiguration AddGoogle(string clientId, string clientSecret, string? callbackPath = null)
    {
        _options.ExternalProviders["Google"] = new ExternalProviderOptions
        {
            IsAvailable = true,
            ClientId = clientId,
            ClientSecret = clientSecret,
            CallbackPath = callbackPath ?? "/signin-google",
            AuthenticationScheme = "Google",
            Icon = "google-icon"
        };
        return this;
    }

    /// <summary>
    /// Add Microsoft provider
    /// </summary>
    public ExternalProvidersConfiguration AddMicrosoft(string clientId, string clientSecret, string? callbackPath = null)
    {
        _options.ExternalProviders["Microsoft"] = new ExternalProviderOptions
        {
            IsAvailable = true,
            ClientId = clientId,
            ClientSecret = clientSecret,
            CallbackPath = callbackPath ?? "/signin-microsoft",
            AuthenticationScheme = "Microsoft",
            Icon = "microsoft-icon"
        };
        return this;
    }

    /// <summary>
    /// Add GitHub provider
    /// </summary>
    public ExternalProvidersConfiguration AddGitHub(string clientId, string clientSecret, string? callbackPath = null, IEnumerable<string>? scopes = null)
    {
        _options.ExternalProviders["GitHub"] = new ExternalProviderOptions
        {
            IsAvailable = true,
            ClientId = clientId,
            ClientSecret = clientSecret,
            CallbackPath = callbackPath ?? "/signin-github",
            AuthenticationScheme = "GitHub",
            Icon = "github-icon",
            Scopes = scopes?.ToList() ?? new List<string> { "read:user", "user:email" }
        };
        return this;
    }

    /// <summary>
    /// Add Facebook provider
    /// </summary>
    public ExternalProvidersConfiguration AddFacebook(string appId, string appSecret, string? callbackPath = null)
    {
        _options.ExternalProviders["Facebook"] = new ExternalProviderOptions
        {
            IsAvailable = true,
            ClientId = appId,
            ClientSecret = appSecret,
            CallbackPath = callbackPath ?? "/signin-facebook",
            AuthenticationScheme = "Facebook",
            Icon = "facebook-icon"
        };
        return this;
    }

    /// <summary>
    /// Add Twitter provider
    /// </summary>
    public ExternalProvidersConfiguration AddTwitter(string clientId, string clientSecret, string? callbackPath = null, IEnumerable<string>? scopes = null)
    {
        _options.ExternalProviders["Twitter"] = new ExternalProviderOptions
        {
            IsAvailable = true,
            ClientId = clientId,
            ClientSecret = clientSecret,
            CallbackPath = callbackPath ?? "/signin-twitter",
            AuthenticationScheme = "Twitter",
            Icon = "twitter-icon",
            Scopes = scopes?.ToList() ?? new List<string> { "tweet.read", "users.read" }
        };
        return this;
    }

    /// <summary>
    /// Add Steam provider
    /// </summary>
    public ExternalProvidersConfiguration AddSteam(string applicationKey, string? callbackPath = null)
    {
        _options.ExternalProviders["Steam"] = new ExternalProviderOptions
        {
            IsAvailable = true,
            ClientId = applicationKey,
            ClientSecret = string.Empty, // Steam doesn't use client secret
            CallbackPath = callbackPath ?? "/signin-steam",
            AuthenticationScheme = "Steam",
            Icon = "steam-icon"
        };
        return this;
    }

    /// <summary>
    /// Add custom provider
    /// </summary>
    public ExternalProvidersConfiguration AddCustom(string providerName, ExternalProviderOptions options)
    {
        _options.ExternalProviders[providerName] = options;
        return this;
    }

    /// <summary>
    /// Return to main options
    /// </summary>
    public IdentityPrvdOptions And()
    {
        return _options;
    }
}
