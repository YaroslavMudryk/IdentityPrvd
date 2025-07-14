using IdentityPrvd.WebApi.Exceptions;
using IdentityPrvd.WebApi.Extensions;
using IdentityPrvd.WebApi.Features.LinkExternalSignin.DataAccess;
using IdentityPrvd.WebApi.UserContext;

namespace IdentityPrvd.WebApi.Features.LinkExternalSignin.Services;

public class UnlinkExternalSigninOrchestrator(
    IUserContext userContext,
    UserLoginRepo userLoginRepo)
{
    public async Task UnlinkExternalProviderFromUserAsync(string provider)
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();

        var userLogin = await userLoginRepo.GetAsync(currentUser.UserId.GetIdAsUlid(), provider)
            ?? throw new BadRequestException($"No linked account found for provider: {provider}");

        await userLoginRepo.HardDeleteAsync(userLogin);
    }
}
