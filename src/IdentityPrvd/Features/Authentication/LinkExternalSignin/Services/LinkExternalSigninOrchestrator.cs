using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace IdentityPrvd.Features.Authentication.LinkExternalSignin.Services;

public class LinkExternalSigninOrchestrator(
    IUserContext userContext,
    IUserLoginQuery userLoginQuery,
    IUserLoginStore userLoginStore)
{
    public async Task LinkExternalProviderToUserAsync(AuthenticateResult authResult)
    {
        if (!authResult.Succeeded)
            throw new BadRequestException("Authentication failed with external provider");

        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();

        var provider = authResult.Principal.Identity.AuthenticationType;
        var userId = authResult.Principal.FindFirstValue(ClaimTypes.NameIdentifier);

        var externalLogin = await userLoginQuery.GetUserLoginByProviderAsync(userId, provider);

        if (externalLogin == null)
        {
            externalLogin = new IdentityUserLogin
            {
                Id = Ulid.NewUlid(),
                UserId = currentUser.UserId.GetIdAsUlid(),
                Provider = provider,
                ProviderUserId = userId
            };
            await userLoginStore.AddAsync(externalLogin);
        }
        else
            throw new BadRequestException("This external account is already linked to another user");
    }
}
