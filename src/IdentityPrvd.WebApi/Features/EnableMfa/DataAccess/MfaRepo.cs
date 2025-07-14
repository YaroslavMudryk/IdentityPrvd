using IdentityPrvd.WebApi.Db;
using IdentityPrvd.WebApi.Db.Entities;
using IdentityPrvd.WebApi.Db.Entities.Enums;
using IdentityPrvd.WebApi.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace IdentityPrvd.WebApi.Features.EnableMfa.DataAccess;

public class MfaRepo(IdentityPrvdContext dbContext)
{
    public async Task<IDbContextTransaction> BeginTransactionAsync() =>
        await dbContext.Database.BeginTransactionAsync();

    public async Task<IdentityUser> GetUserAsync(Ulid userId) =>
        await dbContext.Users.FindAsync(userId) ?? throw new NotFoundException($"User {userId} not found");

    public async Task<IdentityMfa> GetUserMfaTotpNullableAsync(Ulid userId) =>
        await dbContext.Mfas.Where(m => m.Type == MfaType.Totp && m.UserId == userId && m.Activated == null).FirstOrDefaultAsync();

    public async Task<IdentityMfa> GetUserActiveMfaNullableAsync(Ulid userId) =>
        await dbContext.Mfas.Where(m => m.Type == MfaType.Totp && m.UserId == userId && m.Activated != null).FirstOrDefaultAsync();

    public async Task<IdentityMfa> AddAsync(IdentityMfa mfa)
    {
        await dbContext.Mfas.AddAsync(mfa);
        await dbContext.SaveChangesAsync();
        return mfa;
    }

    public async Task<IdentityMfa> UpdateAsync(IdentityMfa mfa)
    {
        if (dbContext.Entry(mfa).State is EntityState.Modified or EntityState.Unchanged)
        {
            await dbContext.SaveChangesAsync();
            return mfa;
        }

        throw new ArgumentException("Entity must be in modified state or unchanged state to be updated.");
    }

    public async Task DeleteAsync(IdentityMfa mfa)
    {
        dbContext.Mfas.Remove(mfa);
        await dbContext.SaveChangesAsync();
    }

    public async Task<IdentityMfa> GetUserActivatedMfaNullableAsync(Ulid userId) =>
        await dbContext.Mfas.AsNoTracking().Where(s => s.Type == MfaType.Totp && s.UserId == userId && s.Activated != null).FirstOrDefaultAsync();

    public async Task<List<IdentityMfaRecoveryCode>> GetMfaRecoveryCodesAsync(Ulid mfaId) =>
        await dbContext.MfaRecoveryCodes.Where(s => s.MfaId == mfaId).ToListAsync();

    public async Task DeleteRecoveryCodesAsync(List<IdentityMfaRecoveryCode> recoveryCodes)
    {
        dbContext.MfaRecoveryCodes.HardRemove(recoveryCodes);
        await dbContext.SaveChangesAsync();
    }
}
