using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace IdentityPrvd.Tests.IntegrationInfra;

[CollectionDefinition(CollectionName, DisableParallelization = true)]
public class PostgresTestWithRedisIntegrationTestFixture : ICollectionFixture<PostgresTestWithRedisWebApplicationFactory>
{
    public const string CollectionName = nameof(PostgresTestWithRedisIntegrationTestFixture);
}

public class IntegrationTestBase(ITestOutputHelper output, IntegrationTestWebApplicationFactory factory)
    : IAsyncLifetime
{
    public ITestOutputHelper Output { get; } = output;
    public IntegrationTestWebApplicationFactory Factory { get; } = factory;

    public virtual Task InitializeAsync()
    {
        Log.Logger = new LoggerConfiguration()
            // add the xunit test output sink to the serilog logger
            // https://github.com/trbenning/serilog-sinks-xunit#serilog-sinks-xunit
            .WriteTo.TestOutput(output)
            .CreateLogger();

        return Task.CompletedTask;
    }

    public virtual async Task DisposeAsync()
    {
        Factory.ResetTimeProvider();
        await Factory.ResetDbAsync();
        
        // Re-initialize system after database reset
        await Factory.InitializeSystemAsync();
    }
}

[Collection(PostgresTestWithRedisIntegrationTestFixture.CollectionName)]
public class PostgresTestWithRedisIntegrationTestBase(ITestOutputHelper output, PostgresTestWithRedisWebApplicationFactory factory)
    : IntegrationTestBase(output, factory)
{
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
    }

    public override async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }
}
