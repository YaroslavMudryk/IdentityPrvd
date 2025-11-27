using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Features.Authorization.Clients.Dtos;

namespace IdentityPrvd.Features.Authorization.Clients.Services;

public class GetClientsOrchestrator(
    IClientsQuery clientsQuery,
    IIdentityContext identityContext)
{
    public async Task<IReadOnlyList<ClientDto>> GetClientsAsync()
    {
        var currentUser = identityContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionOrRoles(
             IdentityPermissions.Clients.Read,
             [DefaultsRoles.Admin]);

        if (currentUser.IsInRoles([DefaultsRoles.Admin]))
            return await clientsQuery.GetAllClientsAsync();
        else
            return await clientsQuery.GetClientsByCreatorIdAsync(currentUser.UserId.GetIdAsGuid());
    }
}
