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
    IClientPermissionsQuery clientPermissionsQuery,
    IIdentityContext identityContext)
{
    public async Task<ClientDto> GetClientAsync(Guid clientId)
    {
        var currentUser = identityContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionOrRoles(
             IdentityPermissions.Clients.Read,
             [DefaultsRoles.Admin]);

        var client = await clientsQuery.GetClientByIdNullableAsync(clientId.GetIdAsString()) ?? throw new NotFoundException($"Client with id:{clientId} not found");

        if (!currentUser.IsInRoles([DefaultsRoles.Admin]) &&
            client.CreatedBy != currentUser.UserId)
            throw new UnauthorizedException("You do not have permission to access this client");

        return await GetEnrichedClientAsync(client);
    }

    private async Task<ClientDto> GetEnrichedClientAsync(IdentityClient client)
    {
        var clientDto = client.MapToDto();
        clientDto.PermissionIds = await clientPermissionsQuery.GetPermissionsIdsByClientIdAsync(client.Id);
        return clientDto;
    }
}
