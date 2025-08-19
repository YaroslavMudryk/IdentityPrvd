using IdentityPrvd.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.DependencyInjection;

public interface IIdentityPrvdBuilder
{
    IServiceCollection Services { get; }
    IConfiguration Configuration { get; }
    IdentityPrvdOptions Options { get; set; }
}

public class IdentityPrvdBuilder(IServiceCollection services) : IIdentityPrvdBuilder
{
    public IdentityPrvdBuilder(IServiceCollection services, IdentityPrvdOptions options) : this(services)
    {
        this.AddCoreServices()
            .AddRequiredServices()
            .AddAuthentication()
            .AddMiddlewares()
            .AddContexts()
            .AddEndpoints()
            .AddDefaultDbContext()
            .AddSessionServices()
            .UseInMemorySessionManagerStore()
            .UseEfTransaction()
            .UseEfStores()
            .UseEfQueries()
            .UseFakeProtectionService()
            .UseFakeHasher()
            .UseFakeEmailNotifier()
            .UseFakeSmsNotifier()
            .UseFakeLocationService();

        services.AddScoped(_ => options);
        Options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public IServiceCollection Services { get; } = services ?? throw new ArgumentNullException(nameof(services));
    public IConfiguration Configuration => Services.BuildServiceProvider().GetRequiredService<IConfiguration>();
    public IdentityPrvdOptions Options { get; set; } = new IdentityPrvdOptions();
}
