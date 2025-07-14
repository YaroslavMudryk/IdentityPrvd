using IdentityPrvd.WebApi.Extensions;
using IdentityPrvd.WebApi.Features.LinkExternalSignin.DataAccess;
using IdentityPrvd.WebApi.Features.LinkExternalSignin.Dtos;
using IdentityPrvd.WebApi.UserContext;

namespace IdentityPrvd.WebApi.Features.LinkExternalSignin.Services;

public class LinkedExternalSigninOrchestrator(
    IUserContext userContext,
    LinkExternalSigninQuery linkExternalSigninQuery)
{
    public async Task<List<ExternalSigninDto>> GetLinkedExternalSigninsAsync()
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        var userLogins = await linkExternalSigninQuery.GetUserLoginsAsync(currentUser.UserId.GetIdAsUlid());

        return [.. ExternalProviders().Select(provider =>
        {
            var isLinked = userLogins.FirstOrDefault(s => s.Provider == provider.Key);

            return new ExternalSigninDto
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
