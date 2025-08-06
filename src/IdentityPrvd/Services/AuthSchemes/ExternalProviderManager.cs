using IdentityPrvd.Extensions;
using IdentityPrvd.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace IdentityPrvd.Services.AuthSchemes;

public class ExternalProviderManager
{
    private readonly Dictionary<string, IExternalProviderConfigurator> _configurators;
    private readonly Dictionary<string, string> _schemeMappings;

    public ExternalProviderManager(IEnumerable<IExternalProviderConfigurator> configurators)
    {
        _configurators = configurators.ToDictionary(c => c.ProviderName, c => c);
        _schemeMappings = new Dictionary<string, string>
        {
            ["Google"] = "Google",
            ["Microsoft"] = "Microsoft",
            ["GitHub"] = "GitHub",
            ["Facebook"] = "Facebook",
            ["Twitter"] = "Twitter",
            ["Steam"] = "Steam"
        };
    }

    public void ConfigureProviders(IdentityAuthenticationBuilder builder, IdentityPrvdOptions identityOptions)
    {
        foreach (var provider in identityOptions.ExternalProviders)
        {
            if (provider.Value.IsAvailable && _configurators.TryGetValue(provider.Key, out var configurator))
            {
                configurator.Configure(builder, provider.Value);
            }
        }
    }

    public async Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string provider)
    {
        if (!_schemeMappings.TryGetValue(provider, out var scheme))
        {
            throw new ArgumentException($"Unsupported provider: {provider}");
        }

        return await context.AuthenticateAsync(scheme);
    }

    public IEnumerable<string> GetAvailableProviders(IdentityPrvdOptions identityOptions)
    {
        return identityOptions.ExternalProviders
            .Where(p => p.Value.IsAvailable)
            .Select(p => p.Key);
    }

    public bool IsProviderAvailable(string provider, IdentityPrvdOptions identityOptions)
    {
        return identityOptions.ExternalProviders.TryGetValue(provider, out var options) && options.IsAvailable;
    }
} 