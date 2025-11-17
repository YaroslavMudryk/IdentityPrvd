using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Queries;

public interface IMfasQuery
{
    Task<IdentityMfa> GetMfaByUserIdAsync(Guid userId);
}

public class EfMfasQuery(IdentityPrvdContext dbContext) : IMfasQuery
{
    public async Task<IdentityMfa> GetMfaByUserIdAsync(Guid userId) =>
        await dbContext.Mfas.AsNoTracking().FirstOrDefaultAsync(s => s.UserId == userId);
}
