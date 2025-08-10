using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.DependencyInjection;

public interface IIdentityPrvdBuilder
{
    IServiceCollection Services { get; }
    IConfiguration Configuration { get; }
}

public class IdentityPrvdBuilder(IServiceCollection services) : IIdentityPrvdBuilder
{
    public IServiceCollection Services { get; } = services ?? throw new ArgumentNullException(nameof(services));
    public IConfiguration Configuration => Services.BuildServiceProvider().GetRequiredService<IConfiguration>();
}
