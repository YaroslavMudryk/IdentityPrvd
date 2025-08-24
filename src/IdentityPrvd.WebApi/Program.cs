using IdentityPrvd.DependencyInjection;
using IdentityPrvd.DependencyInjection.Auth;
using IdentityPrvd.DependencyInjection.Auth.Providers;
using IdentityPrvd.Infrastructure.Database.Context;
using IdentityPrvd.Infrastructure.Database.Seeding;
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
                .UseExternalProviders(providerBuilder =>
                {
                    providerBuilder
                        .AddBattleNet()
                        .AddFacebook()
                        .AddGitHub()
                        .AddGoogle()
                        .AddMicrosoft()
                        .AddCustomProvider<SamsungProvider>()
                        .AddSteam()
                        .AddTwitter()
                        .AddDiscord()
                        .AddApple()
                        .AddSpotify();
                })
                .UseRedisSessionManagerStore(builder.Options.Connections.Redis)
                .UseDbContext<IdentityPrvdContext>(options =>
                {
                    options.UseNpgsql(builder.Options.Connections.Db);
                });
        });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            await IdentityPrvdSeedLoader.InitializeAsync(app.Services);
        }
        
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseIdentityPrvd();

        app.Run();
    }
}
