using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Queries;

public interface IMfasQuery
{
    Task<IdentityMfa> GetMfaByUserIdAsync(Ulid userId);
}

public class EfMfasQuery(IdentityPrvdContext dbContext) : IMfasQuery
{
    public async Task<IdentityMfa> GetMfaByUserIdAsync(Ulid userId) =>
        await dbContext.Mfas.AsNoTracking().FirstOrDefaultAsync(s => s.UserId == userId);
}
