using IdentityPrvd.Infrastructure.Database.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace IdentityPrvd.Tests.IntegrationInfra;

public sealed class TestDatabase : IAsyncDisposable
{
    private Respawner _respawner = default!;
    private NpgsqlConnection _npgsqlConnection = default!;

    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();

    private async Task AddDbResetter()
    {
        _respawner = await Respawner.CreateAsync(_npgsqlConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"]
        });
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        await DisposeAsync();
    }

    public async Task EnsureDbIsCreated(IServiceScope scope)
    {
        await using var dbContext = CreateDbContext(scope);
        await dbContext.Database.EnsureCreatedAsync();
    }

    public Task ResetDbAsync() => _respawner.ResetAsync(_npgsqlConnection);

    public async Task InitializeAsync(IServiceScope scope)
    {
        await _postgreSqlContainer.StartAsync();
        await EnsureDbIsCreated(scope);

        _npgsqlConnection = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString() + ";Include Error Detail=True");
        await _npgsqlConnection.OpenAsync();

        await AddDbResetter();
    }

    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
        await _npgsqlConnection.DisposeAsync();
    }

    public TestIdentityPrvdContext CreateDbContext(IServiceScope scope) => scope?.ServiceProvider.GetRequiredService<TestIdentityPrvdContext>()!;

    public void ConfigureTestServices(IWebHostBuilder builder)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        builder.ConfigureTestServices(serviceCollection => serviceCollection
            .Remove<DbContextOptions<IdentityPrvdContext>>()
            .Replace<IdentityPrvdContext>(s =>
                s.AddDbContext<IdentityPrvdContext>(o
                    => o.UseNpgsql(_postgreSqlContainer.GetConnectionString() + ";Include Error Detail=True"))
            ));

        builder.ConfigureTestServices(serviceCollection => serviceCollection.AddDbContext<TestIdentityPrvdContext>(o =>
                o.UseNpgsql(_postgreSqlContainer.GetConnectionString() + ";Include Error Detail=True")));
    }
}
