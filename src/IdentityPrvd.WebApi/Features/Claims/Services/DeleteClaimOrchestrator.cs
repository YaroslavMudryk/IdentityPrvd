using IdentityPrvd.WebApi.Features.Claims.DataAccess;
using IdentityPrvd.WebApi.Helpers;
using IdentityPrvd.WebApi.UserContext;

namespace IdentityPrvd.WebApi.Features.Claims.Services;

public class DeleteClaimOrchestrator(
    IUserContext userContext,
    ClaimRepo repo)
{
    public async Task DeleteClaimAsync(Ulid claimId)
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionsOrRoles(
            IdentityClaims.Types.Claims, IdentityClaims.Values.Delete,
            [DefaultsRoles.SuperAdmin, DefaultsRoles.Admin]);

        await using var transaction = await repo.BeginTransactionAsync();

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
