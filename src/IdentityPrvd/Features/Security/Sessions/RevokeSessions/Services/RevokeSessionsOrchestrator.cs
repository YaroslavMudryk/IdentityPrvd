using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Data.Transactions;
using IdentityPrvd.Features.Shared.Services;

namespace IdentityPrvd.Features.Security.Sessions.RevokeSessions.Services;

public class RevokeSessionsOrchestrator(
    ISessionControlService sessionControlService,
    IIdentityContext identityContext,
    ISessionStore sessionStore,
    ITransactionManager transactionManager)
{
    public async Task<int> RevokeSessionsAsync(Guid[] sessionIds)
    {
        var currentUser = identityContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissions(
            IdentityClaims.Types.Identity, IdentityClaims.Values.All);

        await using var transaction = await transactionManager.BeginTransactionAsync();

        var sessionsToRevoke = await sessionStore.GetActiveSessionsByIdsAsync(sessionIds);
        if (sessionsToRevoke.Count < sessionIds.Length)
            throw new BadRequestException("Some sessions are unavailable for revoke for certain reasons");

        await sessionControlService.CloseSessionByIdsAsync(sessionIds);
        await transaction.CommitAsync();

        return sessionsToRevoke.Count;
    }

    public async Task RevokeCurrentSessionAsync()
    {
        var currentUser = identityContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissions(
            IdentityClaims.Types.Identity, IdentityClaims.Values.All);

        await using var transaction = await transactionManager.BeginTransactionAsync();
        await sessionControlService.CloseSessionByIdAsync(currentUser.SessionId.GetIdAsGuid());
        await transaction.CommitAsync();
    }
}
