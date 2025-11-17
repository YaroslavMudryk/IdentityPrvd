using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Stores;

namespace IdentityPrvd.Features.Authentication.LinkExternalSignin.Services;

public class UnlinkExternalSigninOrchestrator(
    IIdentityContext identityContext,
    IUserLoginStore userLoginStore)
{
    public async Task UnlinkExternalProviderFromUserAsync(string provider)
    {
        var currentUser = identityContext.AssumeAuthenticated<BasicAuthenticatedUser>();

        var userLogin = await userLoginStore.GetAsync(currentUser.UserId.GetIdAsGuid(), provider)
            ?? throw new BadRequestException($"No linked account found for provider: {provider}");

        await userLoginStore.HardDeleteAsync(userLogin);
    }
}
