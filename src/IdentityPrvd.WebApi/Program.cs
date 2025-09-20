using IdentityPrvd.DependencyInjection;
using IdentityPrvd.DependencyInjection.Auth;
using IdentityPrvd.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.WebApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddIdentityPrvd(builder.Configuration, builder =>
        {
            builder
                .UseSha512Hasher()
                .UseAesProtectionService()
                .UseIpApiLocationService()
                .UseExternalProviders()
                .UseRedisSessionManagerStore(builder.Options.Connections.Redis)
                .UseDbContext<IdentityPrvdContext>(options =>
                {
                    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                    options.UseNpgsql(builder.Options.Connections.Db);
                });
        });

        builder.Services.AddCors(s =>
        {
            s.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseCors();
        app.UseWebSockets();
        app.UseIdentityPrvd();

        await app.RunAsync();
    }
}
