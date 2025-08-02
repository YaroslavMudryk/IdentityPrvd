using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Features.Authentication.ChangeLogin.Dtos;
using IdentityPrvd.Infrastructure.Database.Context;
using IdentityPrvd.Options;
using IdentityPrvd.Services.Security;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Features.Authentication.ChangeLogin.Services;

public class ChangeLoginOrchestrator(
    IUserContext userContext,
    IdentityPrvdContext dbContext,
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

        var userFromDb = await dbContext.Users.FindAsync(userId);

        VerifyLoginType(dto.NewLogin, options);

        if (options.User.VerifyPasswordOnChangeLogin)
        {
            var passwordVerified = hasher.Verify(userFromDb.PasswordHash, dto.Password);
            if (!passwordVerified)
                throw new BadRequestException("Your password is invalid");
        }

        var existsUser = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(s => s.Login == dto.NewLogin);
        if (existsUser != null)
            if (existsUser.Id != userId)
                throw new BadRequestException("This login is busy");

        userFromDb.Login = dto.NewLogin;

        await dbContext.SaveChangesAsync();
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
