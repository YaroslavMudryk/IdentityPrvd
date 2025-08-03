using IdentityPrvd.Options;

namespace IdentityPrvd.Services.AuthSchemes;

public interface IExternalProviderConfigurator
{
    string ProviderName { get; }
    void Configure(IdentityAuthenticationBuilder builder, ExternalProviderOptions options);
} 