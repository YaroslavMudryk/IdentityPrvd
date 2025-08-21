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
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        ExternalProviders = [];
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
        if (User.LoginType == LoginType.Any && User.ConfirmRequired)
            throw new ApplicationException("No confirmation flow available when login is random string");

        if (!Language.UseCustomLanguages)
        {
            if (Language.Languages == null || Language.Languages.Length == 0)
                throw new ApplicationException("Should be at least one language");
        }
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
    public SessionConfiguration AddCustomStore<TSessionStore>() where TSessionStore : class, Infrastructure.Caching.ISessionManagerStore
    {
        _sessionStore = new ServiceDescriptor(typeof(Infrastructure.Caching.ISessionManagerStore), typeof(TSessionStore), ServiceLifetime.Scoped);
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
        _sessionStore = new ServiceDescriptor(typeof(Infrastructure.Caching.ISessionManagerStore), typeof(RedisSessionManagerStore), ServiceLifetime.Scoped);
        return this;
    }

    /// <summary>
    /// Use in-memory sessions (for development)
    /// </summary>
    public SessionConfiguration UseInMemorySessions()
    {
        _sessionManager = new ServiceDescriptor(typeof(ISessionManager), typeof(SessionManager), ServiceLifetime.Scoped);
        _sessionStore = new ServiceDescriptor(typeof(Infrastructure.Caching.ISessionManagerStore), typeof(InMemorySessionManagerStore), ServiceLifetime.Scoped);
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
            services.AddScoped<Infrastructure.Caching.ISessionManagerStore, RedisSessionManagerStore>();
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
    private readonly List<ServiceDescriptor> _queries = [];
    private readonly List<ServiceDescriptor> _stores = [];
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
    /// Map claims query to custom implementation
    /// </summary>
    public DatabaseConfiguration MapClaimsQuery<TClaimsQuery>() where TClaimsQuery : class, IClaimsQuery
    {
        _queries.Add(new ServiceDescriptor(typeof(IClaimsQuery), typeof(TClaimsQuery), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map clients query to custom implementation
    /// </summary>
    public DatabaseConfiguration MapClientsQuery<TClientsQuery>() where TClientsQuery : class, IClientsQuery
    {
        _queries.Add(new ServiceDescriptor(typeof(IClientsQuery), typeof(TClientsQuery), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map refresh tokens query to custom implementation
    /// </summary>
    public DatabaseConfiguration MapRefreshTokensQuery<TRefreshTokensQuery>() where TRefreshTokensQuery : class, IRefreshTokensQuery
    {
        _queries.Add(new ServiceDescriptor(typeof(IRefreshTokensQuery), typeof(TRefreshTokensQuery), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map roles query to custom implementation
    /// </summary>
    public DatabaseConfiguration MapRolesQuery<TRolesQuery>() where TRolesQuery : class, IRolesQuery
    {
        _queries.Add(new ServiceDescriptor(typeof(IRolesQuery), typeof(TRolesQuery), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map sessions query to custom implementation
    /// </summary>
    public DatabaseConfiguration MapSessionsQuery<TSessionsQuery>() where TSessionsQuery : class, ISessionsQuery
    {
        _queries.Add(new ServiceDescriptor(typeof(ISessionsQuery), typeof(TSessionsQuery), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map user logins query to custom implementation
    /// </summary>
    public DatabaseConfiguration MapUserLoginsQuery<TUserLoginsQuery>() where TUserLoginsQuery : class, IUserLoginsQuery
    {
        _queries.Add(new ServiceDescriptor(typeof(IUserLoginsQuery), typeof(TUserLoginsQuery), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map users query to custom implementation
    /// </summary>
    public DatabaseConfiguration MapUsersQuery<TUsersQuery>() where TUsersQuery : class, IUsersQuery
    {
        _queries.Add(new ServiceDescriptor(typeof(IUsersQuery), typeof(TUsersQuery), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map contacts query to custom implementation
    /// </summary>
    public DatabaseConfiguration MapContactsQuery<TContactsQuery>() where TContactsQuery : class, IContactsQuery
    {
        _queries.Add(new ServiceDescriptor(typeof(IContactsQuery), typeof(TContactsQuery), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map devices query to custom implementation
    /// </summary>
    public DatabaseConfiguration MapDevicesQuery<TDevicesQuery>() where TDevicesQuery : class, IDevicesQuery
    {
        _queries.Add(new ServiceDescriptor(typeof(IDevicesQuery), typeof(TDevicesQuery), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map user roles query to custom implementation
    /// </summary>
    public DatabaseConfiguration MapUserRolesQuery<TUserRolesQuery>() where TUserRolesQuery : class, IUserRolesQuery
    {
        _queries.Add(new ServiceDescriptor(typeof(IUserRolesQuery), typeof(TUserRolesQuery), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map claim store to custom implementation
    /// </summary>
    public DatabaseConfiguration MapClaimStore<TClaimStore>() where TClaimStore : class, IClaimStore
    {
        _stores.Add(new ServiceDescriptor(typeof(IClaimStore), typeof(TClaimStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map confirm store to custom implementation
    /// </summary>
    public DatabaseConfiguration MapConfirmStore<TConfirmStore>() where TConfirmStore : class, IConfirmStore
    {
        _stores.Add(new ServiceDescriptor(typeof(IConfirmStore), typeof(TConfirmStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map MFA store to custom implementation
    /// </summary>
    public DatabaseConfiguration MapMfaStore<TMfaStore>() where TMfaStore : class, IMfaStore
    {
        _stores.Add(new ServiceDescriptor(typeof(IMfaStore), typeof(TMfaStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map password store to custom implementation
    /// </summary>
    public DatabaseConfiguration MapPasswordStore<TPasswordStore>() where TPasswordStore : class, IPasswordStore
    {
        _stores.Add(new ServiceDescriptor(typeof(IPasswordStore), typeof(TPasswordStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map refresh token store to custom implementation
    /// </summary>
    public DatabaseConfiguration MapRefreshTokenStore<TRefreshTokenStore>() where TRefreshTokenStore : class, IRefreshTokenStore
    {
        _stores.Add(new ServiceDescriptor(typeof(IRefreshTokenStore), typeof(TRefreshTokenStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map role claim store to custom implementation
    /// </summary>
    public DatabaseConfiguration MapRoleClaimStore<TRoleClaimStore>() where TRoleClaimStore : class, IRoleClaimStore
    {
        _stores.Add(new ServiceDescriptor(typeof(IRoleClaimStore), typeof(TRoleClaimStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map role store to custom implementation
    /// </summary>
    public DatabaseConfiguration MapRoleStore<TRoleStore>() where TRoleStore : class, IRoleStore
    {
        _stores.Add(new ServiceDescriptor(typeof(IRoleStore), typeof(TRoleStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map session store to custom implementation
    /// </summary>
    public DatabaseConfiguration MapSessionStore<TSessionStore>() where TSessionStore : class, Data.Stores.ISessionStore
    {
        _stores.Add(new ServiceDescriptor(typeof(Data.Stores.ISessionStore), typeof(TSessionStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map user login store to custom implementation
    /// </summary>
    public DatabaseConfiguration MapUserLoginStore<TUserLoginStore>() where TUserLoginStore : class, IUserLoginStore
    {
        _stores.Add(new ServiceDescriptor(typeof(IUserLoginStore), typeof(TUserLoginStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map user role store to custom implementation
    /// </summary>
    public DatabaseConfiguration MapUserRoleStore<TUserRoleStore>() where TUserRoleStore : class, IUserRoleStore
    {
        _stores.Add(new ServiceDescriptor(typeof(IUserRoleStore), typeof(TUserRoleStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map user store to custom implementation
    /// </summary>
    public DatabaseConfiguration MapUserStore<TUserStore>() where TUserStore : class, IUserStore
    {
        _stores.Add(new ServiceDescriptor(typeof(IUserStore), typeof(TUserStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map contact store to custom implementation
    /// </summary>
    public DatabaseConfiguration MapContactStore<TContactStore>() where TContactStore : class, IContactStore
    {
        _stores.Add(new ServiceDescriptor(typeof(IContactStore), typeof(TContactStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map device store to custom implementation
    /// </summary>
    public DatabaseConfiguration MapDeviceStore<TDeviceStore>() where TDeviceStore : class, IDeviceStore
    {
        _stores.Add(new ServiceDescriptor(typeof(IDeviceStore), typeof(TDeviceStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map QR store to custom implementation
    /// </summary>
    public DatabaseConfiguration MapQrStore<TQrStore>() where TQrStore : class, IQrStore
    {
        _stores.Add(new ServiceDescriptor(typeof(IQrStore), typeof(TQrStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map MFA recovery code store to custom implementation
    /// </summary>
    public DatabaseConfiguration MapMfaRecoveryCodeStore<TMfaRecoveryCodeStore>() where TMfaRecoveryCodeStore : class, IMfaRecoveryCodeStore
    {
        _stores.Add(new ServiceDescriptor(typeof(IMfaRecoveryCodeStore), typeof(TMfaRecoveryCodeStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map failed login attempt store to custom implementation
    /// </summary>
    public DatabaseConfiguration MapFailedLoginAttemptStore<TFailedLoginAttemptStore>() where TFailedLoginAttemptStore : class, IFailedLoginAttemptStore
    {
        _stores.Add(new ServiceDescriptor(typeof(IFailedLoginAttemptStore), typeof(TFailedLoginAttemptStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map client store to custom implementation
    /// </summary>
    public DatabaseConfiguration MapClientStore<TClientStore>() where TClientStore : class, IClientStore
    {
        _stores.Add(new ServiceDescriptor(typeof(IClientStore), typeof(TClientStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map client secret store to custom implementation
    /// </summary>
    public DatabaseConfiguration MapClientSecretStore<TClientSecretStore>() where TClientSecretStore : class, IClientSecretStore
    {
        _stores.Add(new ServiceDescriptor(typeof(IClientSecretStore), typeof(TClientSecretStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map client claim store to custom implementation
    /// </summary>
    public DatabaseConfiguration MapClientClaimStore<TClientClaimStore>() where TClientClaimStore : class, IClientClaimStore
    {
        _stores.Add(new ServiceDescriptor(typeof(IClientClaimStore), typeof(TClientClaimStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map ban store to custom implementation
    /// </summary>
    public DatabaseConfiguration MapBanStore<TBanStore>() where TBanStore : class, IBanStore
    {
        _stores.Add(new ServiceDescriptor(typeof(IBanStore), typeof(TBanStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map all queries to Dapper implementations
    /// </summary>
    public DatabaseConfiguration UseDapperQueries()
    {
        // This would map all queries to Dapper implementations
        // Implementation would depend on having Dapper query classes
        return this;
    }

    /// <summary>
    /// Map all stores to Dapper implementations
    /// </summary>
    public DatabaseConfiguration UseDapperStores()
    {
        // This would map all stores to Dapper implementations
        // Implementation would depend on having Dapper store classes
        return this;
    }

    /// <summary>
    /// Map all user-related queries to custom implementations
    /// </summary>
    public DatabaseConfiguration MapUserQueries<TUsersQuery, TUserLoginsQuery, TUserRolesQuery>()
        where TUsersQuery : class, IUsersQuery
        where TUserLoginsQuery : class, IUserLoginsQuery
        where TUserRolesQuery : class, IUserRolesQuery
    {
        _queries.Add(new ServiceDescriptor(typeof(IUsersQuery), typeof(TUsersQuery), ServiceLifetime.Scoped));
        _queries.Add(new ServiceDescriptor(typeof(IUserLoginsQuery), typeof(TUserLoginsQuery), ServiceLifetime.Scoped));
        _queries.Add(new ServiceDescriptor(typeof(IUserRolesQuery), typeof(TUserRolesQuery), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map all user-related stores to custom implementations
    /// </summary>
    public DatabaseConfiguration MapUserStores<TUserStore, TUserLoginStore, TUserRoleStore>()
        where TUserStore : class, IUserStore
        where TUserLoginStore : class, IUserLoginStore
        where TUserRoleStore : class, IUserRoleStore
    {
        _stores.Add(new ServiceDescriptor(typeof(IUserStore), typeof(TUserStore), ServiceLifetime.Scoped));
        _stores.Add(new ServiceDescriptor(typeof(IUserLoginStore), typeof(TUserLoginStore), ServiceLifetime.Scoped));
        _stores.Add(new ServiceDescriptor(typeof(IUserRoleStore), typeof(TUserRoleStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map all role-related queries and stores to custom implementations
    /// </summary>
    public DatabaseConfiguration MapRoleServices<TRolesQuery, TRoleStore, TRoleClaimStore>()
        where TRolesQuery : class, IRolesQuery
        where TRoleStore : class, IRoleStore
        where TRoleClaimStore : class, IRoleClaimStore
    {
        _queries.Add(new ServiceDescriptor(typeof(IRolesQuery), typeof(TRolesQuery), ServiceLifetime.Scoped));
        _stores.Add(new ServiceDescriptor(typeof(IRoleStore), typeof(TRoleStore), ServiceLifetime.Scoped));
        _stores.Add(new ServiceDescriptor(typeof(IRoleClaimStore), typeof(TRoleClaimStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map all claim-related queries and stores to custom implementations
    /// </summary>
    public DatabaseConfiguration MapClaimServices<TClaimsQuery, TClaimStore>()
        where TClaimsQuery : class, IClaimsQuery
        where TClaimStore : class, IClaimStore
    {
        _queries.Add(new ServiceDescriptor(typeof(IClaimsQuery), typeof(TClaimsQuery), ServiceLifetime.Scoped));
        _stores.Add(new ServiceDescriptor(typeof(IClaimStore), typeof(TClaimStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map all client-related queries and stores to custom implementations
    /// </summary>
    public DatabaseConfiguration MapClientServices<TClientsQuery, TClientSecretsQuery, TClientClaimsQuery, TClientStore, TClientSecretStore, TClientClaimStore>()
        where TClientsQuery : class, IClientsQuery
        where TClientSecretsQuery : class, IClientSecretsQuery
        where TClientClaimsQuery : class, IClientClaimsQuery
        where TClientStore : class, IClientStore
        where TClientSecretStore : class, IClientSecretStore
        where TClientClaimStore : class, IClientClaimStore
    {
        _queries.Add(new ServiceDescriptor(typeof(IClientsQuery), typeof(TClientsQuery), ServiceLifetime.Scoped));
        _queries.Add(new ServiceDescriptor(typeof(IClientSecretsQuery), typeof(TClientSecretsQuery), ServiceLifetime.Scoped));
        _queries.Add(new ServiceDescriptor(typeof(IClientClaimsQuery), typeof(TClientClaimsQuery), ServiceLifetime.Scoped));
        _stores.Add(new ServiceDescriptor(typeof(IClientStore), typeof(TClientStore), ServiceLifetime.Scoped));
        _stores.Add(new ServiceDescriptor(typeof(IClientSecretStore), typeof(TClientSecretStore), ServiceLifetime.Scoped));
        _stores.Add(new ServiceDescriptor(typeof(IClientClaimStore), typeof(TClientClaimStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map all MFA-related queries and stores to custom implementations
    /// </summary>
    public DatabaseConfiguration MapMfaServices<TMfasQuery, TMfaRecoveryCodesQuery, TMfaStore, TMfaRecoveryCodeStore>()
        where TMfasQuery : class, IMfasQuery
        where TMfaRecoveryCodesQuery : class, IMfaRecoveryCodesQuery
        where TMfaStore : class, IMfaStore
        where TMfaRecoveryCodeStore : class, IMfaRecoveryCodeStore
    {
        _queries.Add(new ServiceDescriptor(typeof(IMfasQuery), typeof(TMfasQuery), ServiceLifetime.Scoped));
        _queries.Add(new ServiceDescriptor(typeof(IMfaRecoveryCodesQuery), typeof(TMfaRecoveryCodesQuery), ServiceLifetime.Scoped));
        _stores.Add(new ServiceDescriptor(typeof(IMfaStore), typeof(TMfaStore), ServiceLifetime.Scoped));
        _stores.Add(new ServiceDescriptor(typeof(IMfaRecoveryCodeStore), typeof(TMfaRecoveryCodeStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map all security-related queries and stores to custom implementations
    /// </summary>
    public DatabaseConfiguration MapSecurityServices<TBansQuery, TFailedLoginAttemptsQuery, TBanStore, TFailedLoginAttemptStore>()
        where TBansQuery : class, IBansQuery
        where TFailedLoginAttemptsQuery : class, IFailedLoginAttemptsQuery
        where TBanStore : class, IBanStore
        where TFailedLoginAttemptStore : class, IFailedLoginAttemptStore
    {
        _queries.Add(new ServiceDescriptor(typeof(IBansQuery), typeof(TBansQuery), ServiceLifetime.Scoped));
        _queries.Add(new ServiceDescriptor(typeof(IFailedLoginAttemptsQuery), typeof(TFailedLoginAttemptsQuery), ServiceLifetime.Scoped));
        _stores.Add(new ServiceDescriptor(typeof(IBanStore), typeof(TBanStore), ServiceLifetime.Scoped));
        _stores.Add(new ServiceDescriptor(typeof(IFailedLoginAttemptStore), typeof(TFailedLoginAttemptStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map all QR-related queries and stores to custom implementations
    /// </summary>
    public DatabaseConfiguration MapQrServices<TQrsQuery, TQrStore>()
        where TQrsQuery : class, IQrsQuery
        where TQrStore : class, IQrStore
    {
        _queries.Add(new ServiceDescriptor(typeof(IQrsQuery), typeof(TQrsQuery), ServiceLifetime.Scoped));
        _stores.Add(new ServiceDescriptor(typeof(IQrStore), typeof(TQrStore), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map bans query to custom implementation
    /// </summary>
    public DatabaseConfiguration MapBansQuery<TBansQuery>() where TBansQuery : class, IBansQuery
    {
        _queries.Add(new ServiceDescriptor(typeof(IBansQuery), typeof(TBansQuery), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map passwords query to custom implementation
    /// </summary>
    public DatabaseConfiguration MapPasswordsQuery<TPasswordsQuery>() where TPasswordsQuery : class, IPasswordsQuery
    {
        _queries.Add(new ServiceDescriptor(typeof(IPasswordsQuery), typeof(TPasswordsQuery), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map role claims query to custom implementation
    /// </summary>
    public DatabaseConfiguration MapRoleClaimsQuery<TRoleClaimsQuery>() where TRoleClaimsQuery : class, IRoleClaimsQuery
    {
        _queries.Add(new ServiceDescriptor(typeof(IRoleClaimsQuery), typeof(TRoleClaimsQuery), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map QR query to custom implementation
    /// </summary>
    public DatabaseConfiguration MapQrsQuery<TQrsQuery>() where TQrsQuery : class, IQrsQuery
    {
        _queries.Add(new ServiceDescriptor(typeof(IQrsQuery), typeof(TQrsQuery), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map MFA recovery codes query to custom implementation
    /// </summary>
    public DatabaseConfiguration MapMfaRecoveryCodesQuery<TMfaRecoveryCodesQuery>() where TMfaRecoveryCodesQuery : class, IMfaRecoveryCodesQuery
    {
        _queries.Add(new ServiceDescriptor(typeof(IMfaRecoveryCodesQuery), typeof(TMfaRecoveryCodesQuery), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map failed login attempts query to custom implementation
    /// </summary>
    public DatabaseConfiguration MapFailedLoginAttemptsQuery<TFailedLoginAttemptsQuery>() where TFailedLoginAttemptsQuery : class, IFailedLoginAttemptsQuery
    {
        _queries.Add(new ServiceDescriptor(typeof(IFailedLoginAttemptsQuery), typeof(TFailedLoginAttemptsQuery), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map MFAs query to custom implementation
    /// </summary>
    public DatabaseConfiguration MapMfasQuery<TMfasQuery>() where TMfasQuery : class, IMfasQuery
    {
        _queries.Add(new ServiceDescriptor(typeof(IMfasQuery), typeof(TMfasQuery), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map confirms query to custom implementation
    /// </summary>
    public DatabaseConfiguration MapConfirmsQuery<TConfirmsQuery>() where TConfirmsQuery : class, IConfirmsQuery
    {
        _queries.Add(new ServiceDescriptor(typeof(IConfirmsQuery), typeof(TConfirmsQuery), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map client secrets query to custom implementation
    /// </summary>
    public DatabaseConfiguration MapClientSecretsQuery<TClientSecretsQuery>() where TClientSecretsQuery : class, IClientSecretsQuery
    {
        _queries.Add(new ServiceDescriptor(typeof(IClientSecretsQuery), typeof(TClientSecretsQuery), ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Map client claims query to custom implementation
    /// </summary>
    public DatabaseConfiguration MapClientClaimsQuery<TClientClaimsQuery>() where TClientClaimsQuery : class, IClientClaimsQuery
    {
        _queries.Add(new ServiceDescriptor(typeof(IClientClaimsQuery), typeof(TClientClaimsQuery), ServiceLifetime.Scoped));
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

    
    private void RegisterDefaultQueries(IServiceCollection services)
    {
        services.TryAddScoped<IClaimsQuery, EfClaimsQuery>();
        services.TryAddScoped<IClientsQuery, EfClientsQuery>();
        services.TryAddScoped<IRefreshTokensQuery, EfRefreshTokensQuery>();
        services.TryAddScoped<IRolesQuery, EfRolesQuery>();
        services.TryAddScoped<ISessionsQuery, EfSessionsQuery>();
        services.TryAddScoped<IUserLoginsQuery, EfUserLoginsQuery>();
        services.TryAddScoped<IUsersQuery, EfUsersQuery>();
        services.TryAddScoped<IContactsQuery, EfContactsQuery>();
        services.TryAddScoped<IDevicesQuery, EfDevicesQuery>();
        services.TryAddScoped<IUserRolesQuery, EfUserRolesQuery>();
        services.TryAddScoped<IBansQuery, EfBansQuery>();
        services.TryAddScoped<IPasswordsQuery, EfPasswordsQuery>();
        services.TryAddScoped<IRoleClaimsQuery, EfRoleClaimsQuery>();
        services.TryAddScoped<IQrsQuery, EfQrsQuery>();
        services.TryAddScoped<IMfaRecoveryCodesQuery, EfMfaRecoveryCodesQuery>();
        services.TryAddScoped<IFailedLoginAttemptsQuery, EfFailedLoginAttemptsQuery>();
        services.TryAddScoped<IMfasQuery, EfMfasQuery>();
        services.TryAddScoped<IConfirmsQuery, EfConfirmsQuery>();
        services.TryAddScoped<IClientSecretsQuery, EfClientSecretsQuery>();
        services.TryAddScoped<IClientClaimsQuery, EfClientClaimsQuery>();
    }

    private void RegisterDefaultStores(IServiceCollection services)
    {
        services.TryAddScoped<IClaimStore, EfClaimStore>();
        services.TryAddScoped<IConfirmStore, EfConfirmStore>();
        services.TryAddScoped<IMfaStore, EfMfaStore>();
        services.TryAddScoped<IPasswordStore, EfPasswordStore>();
        services.TryAddScoped<IRefreshTokenStore, EfRefreshTokenStore>();
        services.TryAddScoped<IRoleClaimStore, EfRoleClaimStore>();
        services.TryAddScoped<IRoleStore, EfRoleStore>();
        services.TryAddScoped<Data.Stores.ISessionStore, EfSessionStore>();
        services.TryAddScoped<IUserLoginStore, EfUserLoginStore>();
        services.TryAddScoped<IUserRoleStore, EfUserRoleStore>();
        services.TryAddScoped<IUserStore, EfUserStore>();
        services.TryAddScoped<IContactStore, EfContactStore>();
        services.TryAddScoped<IDeviceStore, EfDeviceStore>();
        services.TryAddScoped<IQrStore, EfQrStore>();
        services.TryAddScoped<IMfaRecoveryCodeStore, EfMfaRecoveryCodeStore>();
        services.TryAddScoped<IFailedLoginAttemptStore, EfFailedLoginAttemptStore>();
        services.TryAddScoped<IClientStore, EfClientStore>();
        services.TryAddScoped<IClientSecretStore, EfClientSecretStore>();
        services.TryAddScoped<IClientClaimStore, EfClientClaimStore>();
        services.TryAddScoped<IBanStore, EfBanStore>();
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

    // Parameterless overloads to allow fluent provider enabling when credentials are configured via appsettings
    public ExternalProvidersConfiguration AddGoogle()
    {
        if (!_options.ExternalProviders.TryGetValue("Google", out var existing))
        {
            _options.ExternalProviders["Google"] = new ExternalProviderOptions
            {
                IsAvailable = true,
                ClientId = string.Empty,
                ClientSecret = string.Empty,
                CallbackPath = "/signin-google",
                AuthenticationScheme = "Google",
                Icon = "google-icon"
            };
        }
        else
        {
            existing.IsAvailable = true;
            existing.CallbackPath = string.IsNullOrWhiteSpace(existing.CallbackPath) ? "/signin-google" : existing.CallbackPath;
            existing.AuthenticationScheme = string.IsNullOrWhiteSpace(existing.AuthenticationScheme) ? "Google" : existing.AuthenticationScheme;
        }
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

    public ExternalProvidersConfiguration AddMicrosoft()
    {
        if (!_options.ExternalProviders.TryGetValue("Microsoft", out var existing))
        {
            _options.ExternalProviders["Microsoft"] = new ExternalProviderOptions
            {
                IsAvailable = true,
                ClientId = string.Empty,
                ClientSecret = string.Empty,
                CallbackPath = "/signin-microsoft",
                AuthenticationScheme = "Microsoft",
                Icon = "microsoft-icon"
            };
        }
        else
        {
            existing.IsAvailable = true;
            existing.CallbackPath = string.IsNullOrWhiteSpace(existing.CallbackPath) ? "/signin-microsoft" : existing.CallbackPath;
            existing.AuthenticationScheme = string.IsNullOrWhiteSpace(existing.AuthenticationScheme) ? "Microsoft" : existing.AuthenticationScheme;
        }
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
            Scopes = scopes?.ToList() ?? ["read:user", "user:email"]
        };
        return this;
    }

    public ExternalProvidersConfiguration AddGitHub(IEnumerable<string>? scopes = null)
    {
        if (!_options.ExternalProviders.TryGetValue("GitHub", out var existing))
        {
            _options.ExternalProviders["GitHub"] = new ExternalProviderOptions
            {
                IsAvailable = true,
                ClientId = string.Empty,
                ClientSecret = string.Empty,
                CallbackPath = "/signin-github",
                AuthenticationScheme = "GitHub",
                Icon = "github-icon",
                Scopes = scopes?.ToList() ?? ["read:user", "user:email"]
            };
        }
        else
        {
            existing.IsAvailable = true;
            existing.CallbackPath = string.IsNullOrWhiteSpace(existing.CallbackPath) ? "/signin-github" : existing.CallbackPath;
            existing.AuthenticationScheme = string.IsNullOrWhiteSpace(existing.AuthenticationScheme) ? "GitHub" : existing.AuthenticationScheme;
            if (scopes != null)
            {
                existing.Scopes = scopes.ToList();
            }
        }
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

    public ExternalProvidersConfiguration AddFacebook()
    {
        if (!_options.ExternalProviders.TryGetValue("Facebook", out var existing))
        {
            _options.ExternalProviders["Facebook"] = new ExternalProviderOptions
            {
                IsAvailable = true,
                ClientId = string.Empty,
                ClientSecret = string.Empty,
                CallbackPath = "/signin-facebook",
                AuthenticationScheme = "Facebook",
                Icon = "facebook-icon"
            };
        }
        else
        {
            existing.IsAvailable = true;
            existing.CallbackPath = string.IsNullOrWhiteSpace(existing.CallbackPath) ? "/signin-facebook" : existing.CallbackPath;
            existing.AuthenticationScheme = string.IsNullOrWhiteSpace(existing.AuthenticationScheme) ? "Facebook" : existing.AuthenticationScheme;
        }
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
            Scopes = scopes?.ToList() ?? ["tweet.read", "users.read"]
        };
        return this;
    }

    public ExternalProvidersConfiguration AddTwitter(IEnumerable<string>? scopes = null)
    {
        if (!_options.ExternalProviders.TryGetValue("Twitter", out var existing))
        {
            _options.ExternalProviders["Twitter"] = new ExternalProviderOptions
            {
                IsAvailable = true,
                ClientId = string.Empty,
                ClientSecret = string.Empty,
                CallbackPath = "/signin-twitter",
                AuthenticationScheme = "Twitter",
                Icon = "twitter-icon",
                Scopes = scopes?.ToList() ?? ["tweet.read", "users.read"]
            };
        }
        else
        {
            existing.IsAvailable = true;
            existing.CallbackPath = string.IsNullOrWhiteSpace(existing.CallbackPath) ? "/signin-twitter" : existing.CallbackPath;
            existing.AuthenticationScheme = string.IsNullOrWhiteSpace(existing.AuthenticationScheme) ? "Twitter" : existing.AuthenticationScheme;
            if (scopes != null)
            {
                existing.Scopes = scopes.ToList();
            }
        }
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

    public ExternalProvidersConfiguration AddSteam()
    {
        if (!_options.ExternalProviders.TryGetValue("Steam", out var existing))
        {
            _options.ExternalProviders["Steam"] = new ExternalProviderOptions
            {
                IsAvailable = true,
                ClientId = string.Empty,
                ClientSecret = string.Empty,
                CallbackPath = "/signin-steam",
                AuthenticationScheme = "Steam",
                Icon = "steam-icon"
            };
        }
        else
        {
            existing.IsAvailable = true;
            existing.CallbackPath = string.IsNullOrWhiteSpace(existing.CallbackPath) ? "/signin-steam" : existing.CallbackPath;
            existing.AuthenticationScheme = string.IsNullOrWhiteSpace(existing.AuthenticationScheme) ? "Steam" : existing.AuthenticationScheme;
        }
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
