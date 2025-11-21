using Microsoft.AspNetCore.TestHost;
using Redis.OM;
using Redis.OM.Contracts;
using StackExchange.Redis;
using Testcontainers.Redis;

namespace IdentityPrvd.Tests.IntegrationInfra;

public class TestRedis
{
    private readonly RedisContainer _redisContainer = new RedisBuilder().WithImage("redis:8.0").Build();

    public async Task InitializeAsync()
    {
        await _redisContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _redisContainer.DisposeAsync();
    }

    public void ConfigureTestServices(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(serviceCollection =>
        {
            var redisDescriptor = serviceCollection.FirstOrDefault(d => d.ServiceType == typeof(IRedisConnectionProvider));
            if (redisDescriptor != null)
            {
                serviceCollection.Remove(redisDescriptor);

                serviceCollection.AddScoped<IRedisConnectionProvider>(provider =>
                {
                    var conn = _redisContainer.GetConnectionString();
                    return new RedisConnectionProvider($"redis://{conn}");
                });
            }
        });
    }

    public IDatabase CreateDatabase()
    {
        var connection = ConnectionMultiplexer.Connect(_redisContainer.GetConnectionString());
        return connection.GetDatabase();
    }
}
