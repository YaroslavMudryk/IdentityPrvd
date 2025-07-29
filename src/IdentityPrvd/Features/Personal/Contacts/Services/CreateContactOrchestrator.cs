using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Features.Personal.Contacts.Dtos;
using IdentityPrvd.Mappers;

namespace IdentityPrvd.Features.Personal.Contacts.Services;

public class CreateContactOrchestrator(
    IUserContext userContext,
    IContactsQuery contactQuery,
    IContactStore contactStore)
{
    public async Task<ContactDto> CreateContactAsync(CreateContactDto dto)
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissions(IdentityClaims.Types.Identity, IdentityClaims.Values.All);

        var userId = currentUser.UserId.GetIdAsUlid();

        var existContact = await contactQuery.GetByTypeAndValueAsync(dto.Type, dto.Value);
        if (existContact != null && existContact.UserId == userId)
            throw new BadRequestException("The same contact already linked to your account");

        var newContact = new IdentityContact
        {
            Title = dto.Title,
            Value = dto.Value,
            IsMain = false,
            IsConfirmed = false,
            ConfirmedAt = null,
            CanBeDeleted = true,
            Type = dto.Type,
            UserId = userId
        };
        var addedContact = await contactStore.AddAsync(newContact);
        return addedContact.MapToDto();
    }
}
