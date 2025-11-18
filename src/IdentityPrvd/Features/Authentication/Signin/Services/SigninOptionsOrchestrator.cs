using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Features.Authentication.Signin.Dtos;
using IdentityPrvd.Options;

namespace IdentityPrvd.Features.Authentication.Signin.Services;

public class SigninOptionsOrchestrator(
    IUsersQuery usersQuery,
    IMfaStore mfaStore,
    IUserLoginsQuery userLoginsQuery,
    IdentityPrvdOptions identityOptions)
{
    public async Task<SigninUserOptionsDto> GetSigninUserOptionsAsync(string login)
    {
        if (string.IsNullOrWhiteSpace(login))
            throw new BadRequestException("Login is required");

        var user = await usersQuery.GetUserByLoginNullableAsync(login);
        
        // If user doesn't exist, return empty options (don't reveal user existence)
        if (user == null)
        {
            return new SigninUserOptionsDto
            {
                Password = false,
                Passwordless = false,
                LinkedExternalProviders = []
            };
        }

        // Check if user has password
        var hasPassword = identityOptions.Signin.Password && !string.IsNullOrWhiteSpace(user.PasswordHash);

        // Check if user has activated MFA
        var activatedMfa = await mfaStore.GetUserActiveMfaNullableAsync(user.Id);
        var hasPasswordless = identityOptions.Signin.Passwordless && activatedMfa != null;

        // Get linked external providers
        var userLogins = await userLoginsQuery.GetUserLoginsAsync(user.Id);
        var linkedProviders = userLogins.Select(ul => ul.Provider).ToList();

        return new SigninUserOptionsDto
        {
            Password = hasPassword,
            Passwordless = hasPasswordless,
            LinkedExternalProviders = linkedProviders
        };
    }
}
