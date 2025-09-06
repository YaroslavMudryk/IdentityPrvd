using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Queries;

public interface IConfirmsQuery
{
    Task<IdentityConfirm> GetConfirmWithUserByCodeAsync(string code);
}

public class EfConfirmsQuery(IdentityPrvdContext dbContext) : IConfirmsQuery
{
    public async Task<IdentityConfirm> GetConfirmWithUserByCodeAsync(string code) =>
        await dbContext.Confirms
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Code == code);
}
