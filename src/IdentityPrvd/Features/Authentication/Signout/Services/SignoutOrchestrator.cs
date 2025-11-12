using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Transactions;
using IdentityPrvd.Features.Shared.Services;

namespace IdentityPrvd.Features.Authentication.Signout.Services;

public class SignoutOrchestrator(
    IUserContext userContext,
    ITransactionManager transactionManager,
    ISessionControlService sessionControlService)
{
    public async Task SignoutAsync(bool everywhere)
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionsOrRoles(
            IdentityClaims.Types.Identity, IdentityClaims.Values.All,
            [DefaultsRoles.SuperAdmin, DefaultsRoles.Admin]);

        await using var transaction = await transactionManager.BeginTransactionAsync();

        if (!everywhere)
        {
            await HandleSignoutAsync(currentUser);
        }
        else
        {
            await HandleSignoutEverywhereAsync(currentUser);
        }

        await transaction.CommitAsync();
    }

    public async Task HandleSignoutEverywhereAsync(BasicAuthenticatedUser currentUser)
    {
        await sessionControlService.CloseActiveUserSessionsAsync(currentUser.UserId.GetIdAsUlid());
    }

    public async Task HandleSignoutAsync(BasicAuthenticatedUser currentUser)
    {
        var sessionId = currentUser.SessionId.GetIdAsUlid();
        await sessionControlService.CloseSessionByIdAsync(sessionId);
    }
}
