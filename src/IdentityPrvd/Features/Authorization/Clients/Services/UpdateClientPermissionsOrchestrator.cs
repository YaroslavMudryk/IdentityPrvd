using FluentValidation;
using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Common.Helpers;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Data.Transactions;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Features.Authorization.Clients.Dtos;
using IdentityPrvd.Mappers;

namespace IdentityPrvd.Features.Authorization.Clients.Services;

public class UpdateClientPermissionsOrchestrator(
    IValidator<UpdateClientPermissionsDto> validator,
    ITransactionManager transactionManager,
    IClientPermissionStore clientPermissionStore,
    IClientStore clientStore,
    IIdentityContext identityContext)
{
    public async Task<ClientDto> UpdatePermissionsAsync(Guid clientId, UpdateClientPermissionsDto dto)
    {
        var currentUser = identityContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionOrRoles(
             IdentityPermissions.Clients.Manage,
             [DefaultsRoles.Admin]);

        await ValidationHelper.ValidateAndThrowAsync(validator, dto);

        await using var transaction = await transactionManager.BeginTransactionAsync();

        var client = await clientStore.GetAsync(clientId) ?? throw new NotFoundException($"Client with id:{clientId} not found");

        await clientPermissionStore.DeleteByClientIdAsync(clientId);

        var newPermissions = dto.PermissionsIds.Select(permissionId => new IdentityClientPermission
        {
            ClientId = clientId,
            PermissionId = permissionId.GetIdAsGuid()
        }).ToList();
        if (newPermissions.Count != 0)
        {
            await clientPermissionStore.CreateAsync(newPermissions);
        }

        await transaction.CommitAsync();

        return client.MapToDto();
    }
}
