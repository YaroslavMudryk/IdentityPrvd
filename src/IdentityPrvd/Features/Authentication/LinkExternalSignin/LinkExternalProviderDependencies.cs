using IdentityPrvd.Features.Authentication.LinkExternalSignin.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Features.Authentication.LinkExternalSignin;

public static class LinkExternalProviderDependencies
{
    public static void AddLinkExternalProviderDependencies(this IServiceCollection services)
    {
        services.AddScoped<LinkedExternalSigninOrchestrator>();
        services.AddScoped<LinkExternalSigninOrchestrator>();
        services.AddScoped<UnlinkExternalSigninOrchestrator>();
    }
}
