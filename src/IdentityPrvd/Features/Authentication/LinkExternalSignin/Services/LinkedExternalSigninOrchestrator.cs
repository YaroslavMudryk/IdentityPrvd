using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Features.Authentication.LinkExternalSignin.Dtos;

namespace IdentityPrvd.Features.Authentication.LinkExternalSignin.Services;

public class LinkedExternalSigninOrchestrator(
    IUserContext userContext,
    IUserLoginsQuery userLoginsQuery)
{
    public async Task<List<ExternalProviderDto>> GetLinkedExternalSigninsAsync()
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        var userLogins = await userLoginsQuery.GetUserLoginsAsync(currentUser.UserId.GetIdAsUlid());

        return [.. ExternalProviders().Select(provider =>
        {
            var isLinked = userLogins.FirstOrDefault(s => s.Provider == provider.Key);

            return new ExternalProviderDto
            {
                Provider = provider.Key,
                Picture = provider.Value,
                IsLinked = isLinked != null,
                LinkedAt = isLinked?.CreatedAt
            };
        })];
    }


    private static Dictionary<string, string> ExternalProviders() =>
        new()
        {
            {"Google", "/img/google.svg"},
            {"Facebook", "/img/facebook.svg"},
            {"Microsoft", "/img/microsoft.svg"},
            {"Apple", "/img/apple.svg"},
            {"GitHub", "/img/github.svg"},
            {"Twitter", "/img/twitter.svg"},
        };
}
