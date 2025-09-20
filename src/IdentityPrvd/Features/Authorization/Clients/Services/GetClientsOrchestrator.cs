using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Features.Authorization.Clients.Dtos;

namespace IdentityPrvd.Features.Authorization.Clients.Services;

public class GetClientsOrchestrator(
    IClientsQuery clientsQuery,
    IUserContext userContext)
{
    public async Task<IReadOnlyList<ClientDto>> GetClientsAsync()
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionsOrRoles(
            IdentityClaims.Types.Clients, IdentityClaims.Values.View,
            [DefaultsRoles.Admin, DefaultsRoles.SuperAdmin]);

        if (currentUser.IsIsRoles([DefaultsRoles.Admin, DefaultsRoles.SuperAdmin]))
            return await clientsQuery.GetAllClientsAsync();
        else
            return await clientsQuery.GetClientsByCreatorIdAsync(currentUser.UserId.GetIdAsUlid());
    }
}
