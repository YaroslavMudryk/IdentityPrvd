using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Domain.Enums;
using IdentityPrvd.Infrastructure.Database.Context;
using IdentityPrvd.Infrastructure.Database.Extensions;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Stores;

public interface IMfaStore
{
    Task<IdentityUser> GetUserAsync(Ulid userId);
    Task<IdentityMfa> GetByUserIdAsync(Ulid userId);
    Task<IdentityMfa> GetUserMfaTotpNullableAsync(Ulid userId);
    Task<IdentityMfa> GetUserActiveMfaNullableAsync(Ulid userId);
    Task<IdentityMfa> AddAsync(IdentityMfa mfa);
    Task<IdentityMfa> UpdateAsync(IdentityMfa mfa);
    Task DeleteAsync(IdentityMfa mfa);
    Task<IdentityMfa> GetUserActivatedMfaNullableAsync(Ulid userId);
    Task<List<IdentityMfaRecoveryCode>> GetMfaRecoveryCodesAsync(Ulid mfaId);
    Task DeleteRecoveryCodesAsync(List<IdentityMfaRecoveryCode> recoveryCodes);
}

public class EfMfaStore(IdentityPrvdContext dbContext) : IMfaStore
{
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

    public async Task<IdentityMfa> GetByUserIdAsync(Ulid userId) =>
        await dbContext.Mfas.FirstOrDefaultAsync(s => s.UserId == userId);
}
