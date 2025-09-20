using FluentValidation;
using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Data.Transactions;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Features.Authorization.Clients.Dtos;
using IdentityPrvd.Mappers;

namespace IdentityPrvd.Features.Authorization.Clients.Services;

public class UpdateClientOrchestrator(
    IValidator<UpdateClientDto> validator,
    IClientStore clientStore,
    IUserContext userContext)
{
    public async Task<ClientDto> UpdateAsync(Ulid clientId, UpdateClientDto dto)
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionsOrRoles(
            IdentityClaims.Types.Clients, IdentityClaims.Values.Update,
            [DefaultsRoles.Admin, DefaultsRoles.SuperAdmin]);

        await validator.ValidateAndThrowAsync(dto);

        var clientToUpdate = await clientStore.GetAsync(clientId) ?? throw new NotFoundException($"Client with id:{clientId} not found");

        clientToUpdate.Name = dto.Name;
        clientToUpdate.Description = dto.Description;
        clientToUpdate.RedirectUris = dto.RedirectUris;
        clientToUpdate.IsActive = dto.IsActive;
        clientToUpdate.ActiveFrom = dto.ActiveFrom;
        clientToUpdate.ActiveTo = dto.ActiveTo;
        clientToUpdate.ClientId = dto.ClientId;
        clientToUpdate.ClientSecretRequired = dto.ClientSecretRequired;
        clientToUpdate.ShortName = dto.ShortName;
        clientToUpdate.Image = dto.Image;

        var updatedClient = await clientStore.UpdateAsync(clientToUpdate);
        return updatedClient.MapToDto();
    }
}

public class UpdateClientClaimsOrchestrator(
    IValidator<UpdateClientClaimsDto> validator,
    ITransactionManager transactionManager,
    IClientClaimStore clientClaimStore,
    IClientStore clientStore,
    IUserContext userContext)
{
    public async Task<ClientDto> UpdateClaimsAsync(Ulid clientId, UpdateClientClaimsDto dto)
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionsOrRoles(
            IdentityClaims.Types.Clients, IdentityClaims.Values.Update,
            [DefaultsRoles.Admin, DefaultsRoles.SuperAdmin]);

        await validator.ValidateAndThrowAsync(dto);

        await using var transaction = await transactionManager.BeginTransactionAsync();

        var client = await clientStore.GetAsync(clientId) ?? throw new NotFoundException($"Client with id:{clientId} not found");

        await clientClaimStore.DeleteByClientIdAsync(clientId);

        var newClaims = dto.ClaimsIds.Select(claimId => new IdentityClientClaim
        {
            ClientId = clientId,
            ClaimId = claimId.GetIdAsUlid()
        }).ToList();
        if (newClaims.Count != 0)
        {
            await clientClaimStore.CreateAsync(newClaims);
        }

        await transaction.CommitAsync();

        return client.MapToDto();
    }
}
