using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Stores;

public interface IContactStore
{
    Task<IdentityContact> AddAsync(IdentityContact contact);
    Task<IdentityContact> UpdateAsync(IdentityContact contact);
    Task<IdentityContact> GetAsync(Guid contactId);
    Task DeleteAsync(IdentityContact contact);
    Task DeleteAsync(Guid contactId);
}

public class EfContactStore(IdentityPrvdContext dbContext) : IContactStore
{
    public async Task<IdentityContact> AddAsync(IdentityContact contact)
    {
        await dbContext.Contacts.AddAsync(contact);
        await dbContext.SaveChangesAsync();
        return contact;
    }

    public async Task DeleteAsync(IdentityContact contact)
    {
        dbContext.Contacts.Remove(contact);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid contactId)
    {
        var contactToDelete = await GetAsync(contactId);
        dbContext.Contacts.Remove(contactToDelete);
        await dbContext.SaveChangesAsync();
    }

    public async Task<IdentityContact> GetAsync(Guid contactId) =>
        await dbContext.Contacts.Where(s => s.Id == contactId).FirstOrDefaultAsync() ?? throw new NotFoundException($"Contact with id:{contactId} not found");

    public async Task<IdentityContact> UpdateAsync(IdentityContact contact)
    {
        if (dbContext.Entry(contact).State is EntityState.Modified or EntityState.Unchanged)
        {
            await dbContext.SaveChangesAsync();
            return contact;
        }

        throw new ArgumentException("Entity must be in modified state or unchanged state to be updated.");
    }
}
