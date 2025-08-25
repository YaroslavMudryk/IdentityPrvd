using IdentityPrvd.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using StackExchange.Redis;

namespace IdentityPrvd.Tests.IntegrationInfra;

public abstract class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    protected TestDatabase TestDatabase { init; get; } = new();
    protected TestRedis TestRedis { init; get; } = new();
    public HttpClient Client { get; private set; } = default!;
    public FakeTimeProvider FakeTimeProvider { get; private set; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services => services
            .Replace<TimeProvider>(s => s.AddTransient<TimeProvider>(_ => FakeTimeProvider)));

        builder.ConfigureTestServices(services =>
        {
            services.AddIdentityPrvd();
        });

        TestDatabase.ConfigureTestServices(builder);
        TestRedis.ConfigureTestServices(builder);
    }

    public virtual Task InitializeAsync()
    {
        Client = CreateClient();
        return Task.CompletedTask;
    }

    public virtual async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await TestDatabase.DisposeAsync();
        await TestRedis.DisposeAsync();
    }

    public void ResetTimeProvider() => FakeTimeProvider = new();

    public TestIdentityPrvdContext CreateDbContext() => TestDatabase.CreateDbContext(Services.CreateScope());

    public Task ResetDbAsync() => TestDatabase.ResetDbAsync();
}

public class PostgresTestWithRedisWebApplicationFactory : IntegrationTestWebApplicationFactory
{
    public override async Task InitializeAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var databaseInitTask = TestDatabase.InitializeAsync(scope);
        var redisTask = TestRedis.InitializeAsync();

        await Task.WhenAll(databaseInitTask, redisTask);
        await base.InitializeAsync();
    }

    public new TestRedis TestRedis => base.TestRedis;

    public IDatabase CreateRedisDatabase() => TestRedis.CreateDatabase();
}
