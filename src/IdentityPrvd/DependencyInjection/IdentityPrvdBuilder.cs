using IdentityPrvd.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.DependencyInjection;

public interface IIdentityPrvdBuilder
{
    IServiceCollection Services { get; }
    IConfiguration Configuration { get; }
    IdentityPrvdOptions Options { get; set; }
    AuthenticationBuilder AuthenticationBuilder { get; }
    bool UseMemoryCache { get; set; }
}

public class IdentityPrvdBuilder : IIdentityPrvdBuilder
{
    public IdentityPrvdBuilder(IServiceCollection services) : this(services, new IdentityPrvdOptions())
    {

    }

    public IdentityPrvdBuilder(IServiceCollection services, IdentityPrvdOptions options)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
        Options = options ?? throw new ArgumentNullException(nameof(options));
        Options.Language ??= new LanguageOptions();
        Options.Language.EnsureEnglishLanguage();
        services.AddScoped(_ => options);
        AuthenticationBuilder = services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
        
        this.AddCoreServices()
            .AddRequiredServices()
            .AddAuthentication()
            .AddMiddlewares()
            .AddContext()
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
    }

    public IServiceCollection Services { get; }
    public IConfiguration Configuration => Services.BuildServiceProvider().GetRequiredService<IConfiguration>();
    public IdentityPrvdOptions Options { get; set; } = new IdentityPrvdOptions();
    public AuthenticationBuilder AuthenticationBuilder { get; }
    public bool UseMemoryCache { get; set; } = false;
}
