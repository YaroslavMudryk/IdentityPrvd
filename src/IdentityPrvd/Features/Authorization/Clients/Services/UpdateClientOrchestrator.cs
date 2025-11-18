using FluentValidation;
using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Common.Helpers;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Features.Authorization.Clients.Dtos;
using IdentityPrvd.Mappers;

namespace IdentityPrvd.Features.Authorization.Clients.Services;

public class UpdateClientOrchestrator(
    IValidator<UpdateClientDto> validator,
    IClientStore clientStore,
    IIdentityContext identityContext)
{
    public async Task<ClientDto> UpdateAsync(Guid clientId, UpdateClientDto dto)
    {
        var currentUser = identityContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionsOrRoles(
            IdentityClaims.Types.Clients, IdentityClaims.Values.Update,
            [DefaultsRoles.Admin, DefaultsRoles.SuperAdmin]);

        await ValidationHelper.ValidateAndThrowAsync(validator, dto);

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
