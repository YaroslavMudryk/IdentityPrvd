using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Data.Transactions;

namespace IdentityPrvd.Features.Authorization.Clients.Services;

public class DeleteClientOrchestrator(
    ITransactionManager transactionManager,
    IClientStore clientStore,
    IClientPermissionStore clientPermissionStore,
    IClientSecretStore clientSecretStore,
    IIdentityContext identityContext)
{
    public async Task DeleteAsync(Guid clientId)
    {
        var currentUser = identityContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionOrRoles(
             IdentityPermissions.Clients.Manage,
             [DefaultsRoles.Admin]);

        await using var transaction = await transactionManager.BeginTransactionAsync();

        var clientToDelete = await clientStore.GetAsync(clientId)
            ?? throw new NotFoundException($"Client with id:{clientId} not found");
        if (clientToDelete.CreatedBy != currentUser.UserId
            && !currentUser.IsInRoles([DefaultsRoles.Admin]))
            throw new UnauthorizedException("You can only delete clients you have created unless you are an admin or super admin");

        await clientPermissionStore.DeleteByClientIdAsync(clientToDelete.Id);
        await clientSecretStore.DeleteByClientIdAsync(clientToDelete.Id);
        await clientStore.DeleteAsync(clientToDelete);

        await transaction.CommitAsync();
    }
}
