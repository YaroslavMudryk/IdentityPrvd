using IdentityPrvd.Features.Security.Initialize.Dtos;
using IdentityPrvd.Options;
using IdentityPrvd.WebApi;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Time.Testing;
using StackExchange.Redis;
using Xunit;

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

    public virtual async Task InitializeSystemAsync()
    {
        // Override in derived classes if system initialization is needed
        await Task.CompletedTask;
    }

    public void ConfigureOptions(Action<IdentityPrvdOptions> configure)
    {
        var options = GetOptionsFromDi();
        configure(options);
    }

    private IdentityPrvdOptions GetOptionsFromDi()
    {
        try
        {
            using var scope = Services.CreateScope();
            var options = scope.ServiceProvider.GetRequiredService<IdentityPrvdOptions>();            
            return options;
        }
        catch
        {
            return new IdentityPrvdOptions();
        }
    }
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
        
        // Initialize system once after database is created
        await InitializeSystemAsync();
    }

    public override async Task InitializeSystemAsync()
    {
        var request = new InitializeRequestDto { AppVersion = "1.0.0-test" };
        var response = await TestApiRequest.Post("/api/system/initialize")
            .WithPayload(request)
            .SendAsync(Client);

        if (response.StatusCode != 200)
        {
            throw new InvalidOperationException(
                $"Failed to initialize system. Status: {response.StatusCode}, Body: {response.GetBodyAsString()}");
        }
    }

    public new TestRedis TestRedis => base.TestRedis;

    public IDatabase CreateRedisDatabase() => TestRedis.CreateDatabase();
}
