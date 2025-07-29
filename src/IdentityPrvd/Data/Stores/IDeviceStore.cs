using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace IdentityPrvd.Data.Stores;

public interface IDeviceStore
{
    Task<IdentityDevice> GetAsync(Ulid deviceId);
    Task<IdentityDevice> AddAsync(IdentityDevice device);
    Task<IdentityDevice> UpdateAsync(IdentityDevice device);
    Task DeleteAsync(IdentityDevice device);
}

public class EfDeviceStore(IdentityPrvdContext dbContext) : IDeviceStore
{
    public async Task<IdentityDevice> AddAsync(IdentityDevice device)
    {
        await dbContext.Devices.AddAsync(device);
        await dbContext.SaveChangesAsync();
        return device;
    }

    public async Task DeleteAsync(IdentityDevice device)
    {
        dbContext.Devices.Remove(device);
        await dbContext.SaveChangesAsync();
    }

    public async Task<IdentityDevice> GetAsync(Ulid deviceId)
    {
        return await dbContext.Devices.FindAsync(deviceId);
    }

    public async Task<IdentityDevice> UpdateAsync(IdentityDevice device)
    {
        if (dbContext.Entry(device).State is EntityState.Modified or EntityState.Unchanged)
        {
            await dbContext.SaveChangesAsync();
            return device;
        }

        throw new ArgumentException("Entity must be in modified state or unchanged state to be updated.");
    }
}
