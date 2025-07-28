using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Database.Context;
using IdentityPrvd.Infrastructure.Database.Extensions;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Stores;

public interface IRoleStore
{
    Task<IdentityRole> AddAsync(IdentityRole role);
    Task<IdentityRole> UpdateAsync(IdentityRole role);
    Task<List<IdentityRole>> UpdateRangeAsync(List<IdentityRole> roles);
    Task<IdentityRole> GetAsync(Ulid roleId);
    Task<List<IdentityRole>> GetAllRolesAsync();
    Task DeleteAsync(IdentityRole role);
}

public class EfRoleStore(IdentityPrvdContext dbContext) : IRoleStore
{
    public async Task<IdentityRole> AddAsync(IdentityRole role)
    {
        await dbContext.Roles.AddAsync(role);
        await dbContext.SaveChangesAsync();
        return role;
    }

    public async Task<IdentityRole> UpdateAsync(IdentityRole role)
    {
        if (dbContext.Entry(role).State is EntityState.Modified or EntityState.Unchanged)
        {
            await dbContext.SaveChangesAsync();
            return role;
        }

        throw new ArgumentException("Entity must be in modified state or unchanged state to be updated.");
    }

    public async Task<IdentityRole> GetAsync(Ulid roleId) =>
        await dbContext.Roles.Where(s => s.Id == roleId).FirstOrDefaultAsync() ?? throw new NotFoundException($"Role with id:{roleId} not found");

    public async Task DeleteAsync(IdentityRole role)
    {
        dbContext.Roles.HardRemove(role);
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<IdentityRole>> GetAllRolesAsync() =>
        await dbContext.Roles.ToListAsync();

    public async Task<List<IdentityRole>> UpdateRangeAsync(List<IdentityRole> roles)
    {
        if (roles.All(r => dbContext.Entry(r).State is EntityState.Modified or EntityState.Unchanged))
        {
            await dbContext.SaveChangesAsync();
            return roles;
        }

        throw new ArgumentException("Entities must be in modified state or unchanged state to be updated.");
    }
}
