using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Features.Authorization.Clients.Dtos;
using IdentityPrvd.Mappers;

namespace IdentityPrvd.Features.Authorization.Clients.Services;

public class GetClientOrchestrator(
    IClientsQuery clientsQuery,
    IClientClaimsQuery clientClaimsQuery,
    IUserContext userContext)
{
    public async Task<ClientDto> GetClientAsync(Ulid clientId)
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionsOrRoles(
            IdentityClaims.Types.Clients, IdentityClaims.Values.View,
            [DefaultsRoles.Admin, DefaultsRoles.SuperAdmin]);
        var client = await clientsQuery.GetClientByIdNullableAsync(clientId.GetIdAsString()) ?? throw new NotFoundException($"Client with id:{clientId} not found");

        if (!currentUser.IsIsRoles([DefaultsRoles.Admin, DefaultsRoles.SuperAdmin]) &&
            client.CreatedBy != currentUser.UserId)
            throw new UnauthorizedException("You do not have permission to access this client");

        return await GetEnrichedClientAsync(client);
    }

    private async Task<ClientDto> GetEnrichedClientAsync(IdentityClient client)
    {
        var clientDto = client.MapToDto();
        clientDto.ClaimsIds = await clientClaimsQuery.GetClaimsIdsByClientIdAsync(client.Id);
        return clientDto;
    }
}
