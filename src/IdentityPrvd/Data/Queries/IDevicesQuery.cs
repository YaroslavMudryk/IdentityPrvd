using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Features.Personal.Devices.Dtos;
using IdentityPrvd.Infrastructure.Database.Context;
using IdentityPrvd.Mappers;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Queries;

public interface IDevicesQuery
{
    Task<List<DeviceDto>> GetUserDevicesAsync(Guid userId);
    Task<IdentityDevice> GetDeviceByIdentifierAsync(string identifier, Guid userId);
}

public class EfDevicesQuery(IdentityPrvdContext dbContext) : IDevicesQuery
{
    public async Task<IdentityDevice> GetDeviceByIdentifierAsync(string identifier, Guid userId) =>
        await dbContext.Devices
        .AsNoTracking()
        .FirstOrDefaultAsync(s => s.Identifier == identifier && s.UserId == userId);

    public async Task<List<DeviceDto>> GetUserDevicesAsync(Guid userId) =>
        await dbContext.Devices
        .AsNoTracking()
        .Where(s => s.UserId == userId)
        .OrderByDescending(s => s.CreatedAt)
        .ProjectToDto()
        .ToListAsync();
}
