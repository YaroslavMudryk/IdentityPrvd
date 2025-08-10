using IdentityPrvd.DependencyInjection;
using IdentityPrvd.Infrastructure.Database.Seeding;

namespace IdentityPrvd.WebApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddIdentityPrvd(builder.Configuration, options =>
        {
            options.User.ConfirmCodeValidInMinutes = 10;
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
