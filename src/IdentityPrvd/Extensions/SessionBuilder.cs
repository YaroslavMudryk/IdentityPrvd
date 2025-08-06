using IdentityPrvd.Infrastructure.Caching;
using IdentityPrvd.Options;
using IdentityPrvd.Services.ServerSideSessions;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Extensions;

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
        _services.AddScoped<ISessionStore, RedisSessionStore>();
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
    public SessionBuilder UseCustomSessionStore<TSessionStore>() where TSessionStore : class, ISessionStore
    {
        _services.AddScoped<ISessionStore, TSessionStore>();
        return this;
    }

    /// <summary>
    /// Use in-memory session store (for development/testing)
    /// </summary>
    public SessionBuilder UseInMemorySessions()
    {
        _services.AddScoped<ISessionStore, InMemorySessionStore>();
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
