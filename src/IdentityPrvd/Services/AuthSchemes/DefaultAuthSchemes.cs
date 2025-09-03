using IdentityPrvd.Options;
using Microsoft.AspNetCore.Authentication;

namespace IdentityPrvd.Services.AuthSchemes;

public class DefaultAuthSchemes(
    IAuthenticationSchemeProvider authenticationSchemeProvider,
    IdentityPrvdOptions identityOptions) : IAuthSchemes
{
    public async Task<List<AuthSchemeDto>> GetAllSchemesAsync()
    {
        var schemes = await authenticationSchemeProvider.GetAllSchemesAsync();

        var list = new List<AuthSchemeDto>();

        foreach (var scheme in schemes)
        {
            if (scheme.Name == "Bearer" || scheme.Name == "cookie")
                continue;

            if (identityOptions.ExternalProviders.TryGetValue(scheme.Name, out var provider))
                list.Add(new AuthSchemeDto
                {
                    Provider = scheme.Name,
                    Icon = provider.Icon,
                    IsAvailable = provider.IsAvailable,
                });
            else
                list.Add(new AuthSchemeDto
                {
                    Provider = scheme.Name,
                    IsAvailable = false,
                    IsConfigured = false,
                    Icon = null,
                });
        }

        return list;
    }

    public async Task<List<AuthSchemeDto>> GetAvailableSchemesAsync()
    {
        return [.. (await GetAllSchemesAsync()).Where(s => s.IsAvailable)];
    }
}
