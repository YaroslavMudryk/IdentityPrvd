using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace IdentityPrvd.Services.AuthSchemes;

public class ExternalProviderManager
{
    private readonly Dictionary<string, string> _schemeMappings = new Dictionary<string, string>
    {
        ["Google"] = "Google",
        ["Microsoft"] = "Microsoft",
        ["GitHub"] = "GitHub",
        ["Facebook"] = "Facebook",
        ["Twitter"] = "Twitter",
        ["Steam"] = "Steam"
    };

    public async Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string provider)
    {
        if (!_schemeMappings.TryGetValue(provider, out var scheme))
        {
            throw new ArgumentException($"Unsupported provider: {provider}");
        }

        return await context.AuthenticateAsync(scheme);
    }
}