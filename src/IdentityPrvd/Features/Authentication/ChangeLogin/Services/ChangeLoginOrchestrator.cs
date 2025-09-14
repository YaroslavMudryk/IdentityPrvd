using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Features.Authentication.ChangeLogin.Dtos;
using IdentityPrvd.Options;
using IdentityPrvd.Services.Security;

namespace IdentityPrvd.Features.Authentication.ChangeLogin.Services;

public class ChangeLoginOrchestrator(
    IUserContext userContext,
    IUserStore userStore,
    IUsersQuery usersQuery,
    IdentityPrvdOptions options,
    IHasher hasher)
{
    public async Task ChangeLoginAsync(ChangeLoginDto dto)
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionsOrRoles(
            IdentityClaims.Types.Identity, IdentityClaims.Values.All,
            [DefaultsRoles.SuperAdmin, DefaultsRoles.Admin]);
        var userId = currentUser.UserId.GetIdAsUlid();

        var userFromDb = await userStore.GetUserAsync(userId);

        VerifyLoginType(dto.NewLogin, options);

        if (options.User.VerifyPasswordOnChangeLogin)
        {
            var passwordVerified = hasher.Verify(userFromDb.PasswordHash, dto.Password);
            if (!passwordVerified)
                throw new BadRequestException("Your password is invalid");
        }
        var existsUser = await usersQuery.GetUserByLoginNullableAsync(dto.NewLogin);
        if (existsUser != null)
            if (existsUser.Id != userId)
                throw new BadRequestException("This login is busy");

        userFromDb.Login = dto.NewLogin;

        await userStore.UpdateAsync(userFromDb);
    }

    private static void VerifyLoginType(string newLogin, IdentityPrvdOptions options)
    {
        if (options.User.LoginType == LoginType.Email)
            if (!LoginExtensions.IsEmail(newLogin))
                throw new BadRequestException("Your login should be an email address");

        if (options.User.LoginType == LoginType.Phone)
            if (!LoginExtensions.IsPhone(newLogin))
                throw new BadRequestException("Your login should be an phone number");
    }
}
