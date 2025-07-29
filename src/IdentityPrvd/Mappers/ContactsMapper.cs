using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Features.Personal.Contacts.Dtos;
using Riok.Mapperly.Abstractions;

namespace IdentityPrvd.Mappers;

[Mapper]
public static partial class ContactsMapper
{
    public static partial IQueryable<ContactDto> ProjectToDto(this IQueryable<IdentityContact> contacts);
    public static partial ContactDto MapToDto(this IdentityContact contact);
}
