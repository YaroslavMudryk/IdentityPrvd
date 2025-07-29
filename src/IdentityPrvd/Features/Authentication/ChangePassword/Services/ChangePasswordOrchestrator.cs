using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Data.Transactions;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Domain.Enums;
using IdentityPrvd.Features.Authentication.ChangePassword.Dtos;
using IdentityPrvd.Options;
using IdentityPrvd.Services.Security;
using IdentityPrvd.Services.ServerSideSessions;

namespace IdentityPrvd.Features.Authentication.ChangePassword.Services;

public class ChangePasswordOrchestrator(
    IUserContext userContext,
    IdentityPrvdOptions options,
    TimeProvider timeProvider,
    IUserStore userStore,
    IPasswordStore passwordStore,
    ISessionStore sessionStore,
    IRefreshTokenStore refreshTokenStore,
    ISessionManager sessionManager,
    ITransactionManager transactionManager,
    IHasher hasher)
{
    public async Task ChangePasswordAsync(ChangePasswordDto dto)
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionsOrRoles(
            IdentityClaims.Types.Identity, IdentityClaims.Values.All,
            [DefaultsRoles.SuperAdmin, DefaultsRoles.Admin]);

        var userId = currentUser.UserId.GetIdAsUlid();

        await using var transaction = await transactionManager.BeginTransactionAsync();

        var userFromDb = await userStore.GetUserAysnc(userId);

        if (!string.IsNullOrEmpty(userFromDb.PasswordHash))
        {
            if (!hasher.Verify(userFromDb.PasswordHash, dto.OldPassword))
                throw new BadRequestException("Password not correct");
        }

        var passwordHash = hasher.GetHash(dto.NewPassword);

        IdentityPassword activePassword = null;

        if (!options.UserOptions.UseOldPasswords)
        {
            var passwords = await passwordStore.GetAllUserPasswordsAsync(userId);
            foreach (var password in passwords.Where(s => !s.IsActive))
            {
                if (!hasher.Verify(password.PasswordHash, passwordHash))
                    throw new BadRequestException("Can't be used previews password");
            }
            activePassword = passwords.FirstOrDefault(s => s.IsActive);
        }

        activePassword ??= await passwordStore.GetUserActivePasswordAsync(userId);

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        activePassword.IsActive = false;
        activePassword.DeactivatedAt = utcNow;

        var newUserPassword = new IdentityPassword
        {
            IsActive = true,
            ActivatedAt = utcNow,
            Hint = dto.Hint,
            PasswordHash = passwordHash
        };
        userFromDb.PasswordHash = passwordHash;

        await passwordStore.AddAsync(newUserPassword);

        if (options.UserOptions.ForceSignoutEverywhere || dto.SignoutEverywhere)
        {
            var userSessions = await sessionStore.GetActiveSessionsByUserIdAsync(userId);
            var sessionId = currentUser.SessionId.GetIdAsUlid();
            foreach (var session in userSessions)
            {
                session.Status = SessionStatus.Close;
                session.DeactivatedAt = utcNow;
                session.DeactivatedBySessionId = sessionId;
                await sessionStore.UpdateAsync(session);

                var refreshTokens = await refreshTokenStore.GetRefreshTokensBySessionIdAsync(session.Id);
                foreach (var refreshToken in refreshTokens)
                {
                    refreshToken.UsedAt = utcNow;
                }
                await refreshTokenStore.UpdateRangeAsync(refreshTokens);
                await sessionManager.DeleteSessionAsync(session.UserId.GetIdAsString(), sessionId.GetIdAsString());
            }
        }

        await transaction.CommitAsync();
    }
}
