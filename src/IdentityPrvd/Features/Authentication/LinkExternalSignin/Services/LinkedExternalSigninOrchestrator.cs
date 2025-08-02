using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Features.Authentication.LinkExternalSignin.Dtos;
using IdentityPrvd.Services.AuthSchemes;

namespace IdentityPrvd.Features.Authentication.LinkExternalSignin.Services;

public class LinkedExternalSigninOrchestrator(
    IUserContext userContext,
    IAuthSchemes authSchemes,
    IUserLoginsQuery userLoginsQuery)
{
    public async Task<List<ExternalProviderDto>> GetLinkedExternalSigninsAsync()
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        var userLogins = await userLoginsQuery.GetUserLoginsAsync(currentUser.UserId.GetIdAsUlid());

        var externalProviders = await authSchemes.GetAllSchemesAsync();

        return [.. externalProviders.Select(provider =>
        {
            var isLinked = userLogins.FirstOrDefault(s => s.Provider == provider.Provider);

            return new ExternalProviderDto
            {
                Provider = provider.Provider,
                Picture = provider.Icon,
                IsLinked = isLinked != null,
                LinkedAt = isLinked?.CreatedAt
            };
        })];
    }
}
