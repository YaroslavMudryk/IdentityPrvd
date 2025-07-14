using IdentityPrvd.WebApi.Db;
using IdentityPrvd.WebApi.Db.Entities;
using IdentityPrvd.WebApi.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Runtime.Intrinsics.X86;

namespace IdentityPrvd.WebApi.Features.Roles.DataAccess;

public class RoleRepo(IdentityPrvdContext dbContext)
{
    public async Task<IDbContextTransaction> BeginTransactionAsync() =>
        await dbContext.Database.BeginTransactionAsync();

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
}
