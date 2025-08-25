using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Redis.OM;
using Redis.OM.Contracts;
using StackExchange.Redis;
using Testcontainers.Redis;

namespace IdentityPrvd.Tests.IntegrationInfra;

public class TestRedis
{
    private readonly RedisContainer _redisContainer = new RedisBuilder().Build();

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
                    return new RedisConnectionProvider(_redisContainer.GetConnectionString());
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
