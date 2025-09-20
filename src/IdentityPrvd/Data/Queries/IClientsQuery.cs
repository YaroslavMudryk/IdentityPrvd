using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Features.Authorization.Clients.Dtos;
using IdentityPrvd.Infrastructure.Database.Context;
using IdentityPrvd.Mappers;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Queries;

public interface IClientsQuery
{
    Task<IdentityClient> GetClientByIdAsync(string clientId);
    Task<IdentityClientSecret> GetClientSecretAsync(string clientId);
    Task<IdentityClient> GetClientByIdNullableAsync(string clientId);
    Task<IdentityClientSecret> GetClientSecretNullableAsync(string clientId);
    Task<bool> IsExistsClientAsync();
    Task<IReadOnlyList<ClientDto>> GetAllClientsAsync();
    Task<IReadOnlyList<ClientDto>> GetClientsByCreatorIdAsync(Ulid userId);
}

public class EfClientsQuery(IdentityPrvdContext dbContext) : IClientsQuery
{
    public async Task<IdentityClientSecret> GetClientSecretAsync(string id)
    {
        return await dbContext.ClientSecrets.AsNoTracking().FirstOrDefaultAsync(s => s.IsActive && s.ClientId == id.GetIdAsUlid());
    }

    public async Task<IdentityClient> GetClientByIdNullableAsync(string clientId)
    {
        return (await dbContext.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.ClientId == clientId))!;
    }

    public async Task<IdentityClientSecret> GetClientSecretNullableAsync(string clientId)
    {
        return (await dbContext.ClientSecrets.AsNoTracking().Where(c => c.ClientId == clientId.GetIdAsUlid()).FirstOrDefaultAsync())!;
    }

    public async Task<IdentityClient> GetClientByIdAsync(string clientId) =>
        await dbContext.Clients
        .AsNoTracking()
        .SingleOrDefaultAsync(s => s.ClientId == clientId);

    public async Task<bool> IsExistsClientAsync() =>
        await dbContext.Clients.AsNoTracking().AnyAsync();

    public async Task<IReadOnlyList<ClientDto>> GetAllClientsAsync() =>
        await dbContext.Clients
        .OrderByDescending(s => s.CreatedBy).ThenBy(s => s.Id)
        .ProjectToDto()
        .ToListAsync();

    public async Task<IReadOnlyList<ClientDto>> GetClientsByCreatorIdAsync(Ulid userId) =>
        await dbContext.Clients
        .Where(s => s.CreatedBy == userId.GetIdAsString())
        .OrderByDescending(s => s.CreatedBy).ThenBy(s => s.Id)
        .ProjectToDto()
        .ToListAsync();
}
