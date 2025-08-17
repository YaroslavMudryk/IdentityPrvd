using IdentityPrvd.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.DependencyInjection;

public interface IIdentityPrvdBuilder
{
    IServiceCollection Services { get; }
    IConfiguration Configuration { get; }
    IdentityPrvdOptions Option { get; set; }
}

public class IdentityPrvdBuilder(IServiceCollection services) : IIdentityPrvdBuilder
{
    public IServiceCollection Services { get; } = services ?? throw new ArgumentNullException(nameof(services));
    public IConfiguration Configuration => Services.BuildServiceProvider().GetRequiredService<IConfiguration>();
    public IdentityPrvdOptions Option { get; set; }
}

public interface IIdentityPrvdAuthBuilder : IIdentityPrvdBuilder
{
    AuthenticationBuilder AuthenticationBuilder { get; set; }
}

public class IdentityPrvdAuthBuilder(IServiceCollection services, AuthenticationBuilder builder) : IIdentityPrvdAuthBuilder
{
    public AuthenticationBuilder AuthenticationBuilder { get; set; } = builder ?? throw new ArgumentNullException(nameof(builder));
    public IServiceCollection Services => services ?? throw new ArgumentNullException(nameof(services));
    public IConfiguration Configuration => Services.BuildServiceProvider().GetRequiredService<IConfiguration>();
    public IdentityPrvdOptions Option { get; set; }
}
