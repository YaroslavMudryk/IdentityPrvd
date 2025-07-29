using IdentityPrvd.Common.Constants;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Data.Transactions;

namespace IdentityPrvd.Features.Authorization.Claims.Services;

public class DeleteClaimOrchestrator(
    IUserContext userContext,
    ITransactionManager transactionManager,
    IClaimStore repo)
{
    public async Task DeleteClaimAsync(Ulid claimId)
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionsOrRoles(
            IdentityClaims.Types.Claims, IdentityClaims.Values.Delete,
            [DefaultsRoles.SuperAdmin, DefaultsRoles.Admin]);

        await using var transaction = await transactionManager.BeginTransactionAsync();

        await DeleteClaimReferencesAsync(claimId);
        var claim = await repo.GetAsync(claimId);
        await repo.DeleteAsync(claim);

        await transaction.CommitAsync();
    }

    private async Task DeleteClaimReferencesAsync(Ulid claimId)
    {
        var clientClaimsToDelete = await repo.GetClientClaimsByIdAsync(claimId);
        await repo.DeleteClientClaimsAsync(clientClaimsToDelete);

        var roleClaimsToDelete = await repo.GetRoleClaimsByIdAsync(claimId);
        await repo.DeleteRoleClaimsAsync(roleClaimsToDelete);
    }
}
