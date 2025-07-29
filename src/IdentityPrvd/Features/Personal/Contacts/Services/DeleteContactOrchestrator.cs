using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Stores;

namespace IdentityPrvd.Features.Personal.Contacts.Services;

public class DeleteContactOrchestrator(
    IUserContext userContext,
    IContactStore contactStore)
{
    public async Task DeleteContactAsync(Ulid id)
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissions(IdentityClaims.Types.Identity, IdentityClaims.Values.All);

        var contactToDelete = await contactStore.GetAsync(id);
        if (contactToDelete.UserId != currentUser.UserId.GetIdAsUlid())
            throw new BadRequestException("Contact not anssigne to you");

        if(!contactToDelete.CanBeDeleted)
            throw new BadRequestException("Contact can not be deleted for some reasons");

        await contactStore.DeleteAsync(contactToDelete);
    }
}
