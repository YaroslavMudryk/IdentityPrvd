using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace IdentityPrvd.Services.AuthSchemes;

public class ExternalProviderManager(IAuthSchemes authSchemes)
{
    public async Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string provider)
    {
        var schemeMappings = await authSchemes.GetAvailableSchemesAsync();

        if (!schemeMappings.Any(s => s.Provider.Equals(provider, StringComparison.CurrentCultureIgnoreCase)))
        {
            throw new ArgumentException($"Unsupported provider: {provider}");
        }

        return await context.AuthenticateAsync(provider);
    }
}