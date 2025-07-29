using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Features.Personal.Contacts.Dtos;

namespace IdentityPrvd.Features.Personal.Contacts.Services;

public class GetContactsOrchestrator(
    IUserContext userContext,
    IContactsQuery query)
{
    public async Task<List<ContactDto>> GetContactsAsync()
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissions(IdentityClaims.Types.Identity, IdentityClaims.Values.All);

        return await query.GetUserContactsAsync(currentUser.UserId.GetIdAsUlid());
    }
}
