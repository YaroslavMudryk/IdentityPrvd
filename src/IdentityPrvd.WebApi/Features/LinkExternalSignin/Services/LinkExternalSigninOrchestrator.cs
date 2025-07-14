using IdentityPrvd.WebApi.Db.Entities;
using IdentityPrvd.WebApi.Exceptions;
using IdentityPrvd.WebApi.Extensions;
using IdentityPrvd.WebApi.Features.LinkExternalSignin.DataAccess;
using IdentityPrvd.WebApi.UserContext;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace IdentityPrvd.WebApi.Features.LinkExternalSignin.Services;

public class LinkExternalSigninOrchestrator(
    IUserContext userContext,
    LinkExternalSigninQuery linkExternalSigninQuery,
    UserLoginRepo userLoginRepo)
{
    public async Task LinkExternalProviderToUserAsync(AuthenticateResult authResult)
    {
        if (!authResult.Succeeded)
            throw new BadRequestException("Authentication failed with external provider");

        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();

        var provider = authResult.Principal.Identity.AuthenticationType;
        var userId = authResult.Principal.FindFirstValue(ClaimTypes.NameIdentifier);

        var externalLogin = await linkExternalSigninQuery.GetUserLoginByProviderAsync(userId, provider);

        if (externalLogin == null)
        {
            externalLogin = new IdentityUserLogin
            {
                Id = Ulid.NewUlid(),
                UserId = currentUser.UserId.GetIdAsUlid(),
                Provider = provider,
                ProviderUserId = userId
            };
            await userLoginRepo.AddAsync(externalLogin);
        }
        else
            throw new BadRequestException("This external account is already linked to another user");
    }
}
