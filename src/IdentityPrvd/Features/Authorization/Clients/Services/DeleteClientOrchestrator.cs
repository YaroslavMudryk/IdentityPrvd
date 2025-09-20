using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Data.Transactions;

namespace IdentityPrvd.Features.Authorization.Clients.Services;

public class DeleteClientOrchestrator(
    ITransactionManager transactionManager,
    IClientStore clientStore,
    IClientClaimStore clientClaimStore,
    IClientSecretStore clientSecretStore,
    IUserContext userContext)
{
    public async Task DeleteAsync(Ulid clientId)
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionsOrRoles(
            IdentityClaims.Types.Clients, IdentityClaims.Values.Delete,
            [DefaultsRoles.Admin, DefaultsRoles.SuperAdmin]);

        await using var transaction = await transactionManager.BeginTransactionAsync();

        var clientToDelete = await clientStore.GetAsync(clientId)
            ?? throw new NotFoundException($"Client with id:{clientId} not found");
        if (clientToDelete.CreatedBy != currentUser.UserId
            && !currentUser.IsIsRoles([DefaultsRoles.Admin, DefaultsRoles.SuperAdmin]))
            throw new UnauthorizedException("You can only delete clients you have created unless you are an admin or super admin");

        await clientClaimStore.DeleteByClientIdAsync(clientToDelete.Id);
        await clientSecretStore.DeleteByClientIdAsync(clientToDelete.Id);
        await clientStore.DeleteAsync(clientToDelete);

        await transaction.CommitAsync();
    }
}
