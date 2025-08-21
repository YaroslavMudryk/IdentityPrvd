using IdentityPrvd.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.DependencyInjection.Auth;

public interface IExternalProvidersBuilder
{
    AuthenticationBuilder Authentication { get; }
    IdentityPrvdOptions Options { get; }
    IExternalProvidersBuilder AddCustomProvider<TProvider>() where TProvider : class, ICustomExternalProvider, new();
}

internal class ExternalProvidersBuilder(IIdentityPrvdBuilder builder) : IExternalProvidersBuilder
{
    private readonly IIdentityPrvdBuilder _builder = builder;
    public AuthenticationBuilder Authentication => _builder.AuthenticationBuilder;
    public IdentityPrvdOptions Options => _builder.Options;

    public IExternalProvidersBuilder AddCustomProvider<TProvider>() where TProvider : class, ICustomExternalProvider, new()
    {
        _builder.Services.AddScoped<ICustomExternalProvider, TProvider>();
        var provider = new TProvider();
        provider.Register(_builder.AuthenticationBuilder, _builder.Options);
        return this;
    }
}
