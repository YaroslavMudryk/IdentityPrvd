using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Features.Personal.Contacts.Dtos;

namespace IdentityPrvd.Features.Personal.Contacts.Services;

public class GetContactsOrchestrator(
    IIdentityContext identityContext,
    IContactsQuery query)
{
    public async Task<List<ContactDto>> GetContactsAsync()
    {
        var currentUser = identityContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermission(IdentityPermissions.Contacts.Read);

        return await query.GetUserContactsAsync(currentUser.UserId.GetIdAsGuid());
    }
}
