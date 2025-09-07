using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Queries;

public interface IConfirmsQuery
{
    Task<IdentityCode> GetConfirmWithUserByCodeAsync(string code);
}

public class EfConfirmsQuery(IdentityPrvdContext dbContext) : IConfirmsQuery
{
    public async Task<IdentityCode> GetConfirmWithUserByCodeAsync(string code) =>
        await dbContext.Confirms
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Code == code);
}
