using FluentValidation;
using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Helpers;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Data.Transactions;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Features.Authorization.Clients.Dtos;
using IdentityPrvd.Mappers;
using IdentityPrvd.Services.Security;

namespace IdentityPrvd.Features.Authorization.Clients.Services;

public class CreateClientOrchestrator(
    IValidator<CreateClientDto> validator,
    ITransactionManager transactionManager,
    IHasher hasher,
    IClientStore clientStore,
    IUserContext userContext)
{
    public async Task<ClientDto> CreateAsync(CreateClientDto dto)
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionsOrRoles(
            IdentityClaims.Types.Clients, IdentityClaims.Values.Create,
            [DefaultsRoles.Admin, DefaultsRoles.SuperAdmin]);

        await validator.ValidateAndThrowAsync(dto);

        await using var transaction = await transactionManager.BeginTransactionAsync();

        var newClient = dto.MapToEntity();
        newClient.ClientId = string.IsNullOrWhiteSpace(dto.ClientId) ? Generator.GetString(16) : dto.ClientId;
        var secret = Generator.GetString(32);
        if (newClient.ClientSecretRequired)
        {
            newClient.ClientSecrets = GetClientSecret(secret);
        }

        var createdClient = await clientStore.AddAsync(newClient);
        await transaction.CommitAsync();

        return createdClient.MapToDto(secret);
    }

    private List<IdentityClientSecret> GetClientSecret(string secret)
    {
        return
        [
            new()
            {
                Id = Ulid.NewUlid(),
                IsActive = true,
                Value = hasher.GetHash(secret),
            }
        ];
    }
}
