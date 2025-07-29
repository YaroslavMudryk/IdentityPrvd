using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Domain.Enums;
using IdentityPrvd.Features.Personal.Contacts.Dtos;
using IdentityPrvd.Infrastructure.Database.Context;
using IdentityPrvd.Mappers;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Queries;

public interface IContactsQuery
{
    Task<List<ContactDto>> GetUserContactsAsync(Ulid userId, bool withDeleted = false);
    Task<IdentityContact> GetByTypeAndValueAsync(ContactType type, string value); 
}

public class EfContactsQuery(IdentityPrvdContext dbContext) : IContactsQuery
{
    public async Task<IdentityContact> GetByTypeAndValueAsync(ContactType type, string value) =>
        await dbContext.Contacts.AsNoTracking().FirstOrDefaultAsync(s => s.Type == type && s.Value == value);

    public async Task<List<ContactDto>> GetUserContactsAsync(Ulid userId, bool withDeleted = false)
    {
        IQueryable<IdentityContact> query = dbContext.Contacts
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(s => s.CreatedAt);

        if (withDeleted)
            query = query.IgnoreQueryFilters();

        return await query.ProjectToDto().ToListAsync();
    }
}
