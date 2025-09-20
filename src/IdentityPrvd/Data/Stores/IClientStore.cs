using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Database.Context;

namespace IdentityPrvd.Data.Stores;

public interface IClientStore
{
    Task<IdentityClient> GetAsync(Ulid clientId);
    Task<IdentityClient> AddAsync(IdentityClient client);
    Task<IdentityClient> UpdateAsync(IdentityClient client);
    Task DeleteAsync(IdentityClient client);
}

public class EfClientStore(IdentityPrvdContext dbContext) : IClientStore
{
    public async Task<IdentityClient> AddAsync(IdentityClient client)
    {
        await dbContext.Clients.AddAsync(client);
        await dbContext.SaveChangesAsync();
        return client;
    }

    public async Task DeleteAsync(IdentityClient client)
    {
        dbContext.Clients.Remove(client);
        await dbContext.SaveChangesAsync();
    }

    public async Task<IdentityClient> GetAsync(Ulid clientId) =>
        await dbContext.Clients.FindAsync(clientId);

    public async Task<IdentityClient> UpdateAsync(IdentityClient client)
    {
        if (dbContext.Entry(client).State == Microsoft.EntityFrameworkCore.EntityState.Detached)
        {
            dbContext.Clients.Update(client);
            await dbContext.SaveChangesAsync();
            return client;
        }

        throw new InvalidOperationException("The entity is already being tracked by the context.");
    }
}
