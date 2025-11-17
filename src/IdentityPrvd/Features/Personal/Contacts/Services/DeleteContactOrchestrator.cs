using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Stores;

namespace IdentityPrvd.Features.Personal.Contacts.Services;

public class DeleteContactOrchestrator(
    IIdentityContext identityContext,
    IContactStore contactStore)
{
    public async Task DeleteContactAsync(Guid id)
    {
        var currentUser = identityContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissions(IdentityClaims.Types.Identity, IdentityClaims.Values.All);

        var contactToDelete = await contactStore.GetAsync(id);
        if (contactToDelete.UserId != currentUser.UserId.GetIdAsGuid())
            throw new BadRequestException("Contact not anssigne to you");

        if(!contactToDelete.CanBeDeleted)
            throw new BadRequestException("Contact can not be deleted for some reasons");

        await contactStore.DeleteAsync(contactToDelete);
    }
}
