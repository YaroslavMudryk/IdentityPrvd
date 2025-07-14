using IdentityPrvd.WebApi.Extensions;
using IdentityPrvd.WebApi.Seeding;

namespace IdentityPrvd.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddIdentityPrvd(builder.Configuration, options =>
        {
            options.UserOptions.ConfirmCodeValidInMinutes = 10;
        });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            SeedData.InitializeAsync(app.Services).GetAwaiter().GetResult();
        }

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseIdentityPrvd();

        app.Run();
    }
}
