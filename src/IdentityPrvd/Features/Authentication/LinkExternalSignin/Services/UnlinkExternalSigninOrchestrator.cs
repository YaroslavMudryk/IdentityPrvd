using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Stores;

namespace IdentityPrvd.Features.Authentication.LinkExternalSignin.Services;

public class UnlinkExternalSigninOrchestrator(
    IUserContext userContext,
    IUserLoginStore userLoginStore)
{
    public async Task UnlinkExternalProviderFromUserAsync(string provider)
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();

        var userLogin = await userLoginStore.GetAsync(currentUser.UserId.GetIdAsUlid(), provider)
            ?? throw new BadRequestException($"No linked account found for provider: {provider}");

        await userLoginStore.HardDeleteAsync(userLogin);
    }
}
