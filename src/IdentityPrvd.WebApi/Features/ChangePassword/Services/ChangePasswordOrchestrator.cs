using IdentityPrvd.WebApi.Db.Entities;
using IdentityPrvd.WebApi.Db.Entities.Enums;
using IdentityPrvd.WebApi.Exceptions;
using IdentityPrvd.WebApi.Extensions;
using IdentityPrvd.WebApi.Features.ChangePassword.DataAccess;
using IdentityPrvd.WebApi.Features.ChangePassword.Dtos;
using IdentityPrvd.WebApi.Helpers;
using IdentityPrvd.WebApi.Options;
using IdentityPrvd.WebApi.Protections;
using IdentityPrvd.WebApi.ServerSideSessions;
using IdentityPrvd.WebApi.UserContext;

namespace IdentityPrvd.WebApi.Features.ChangePassword.Services;

public class ChangePasswordOrchestrator(
    IUserContext userContext,
    IdentityPrvdOptions options,
    TimeProvider timeProvider,
    UserRepo userRepo,
    PasswordRepo passwordRepo,
    SessionRepo sessionRepo,
    RefreshTokenRepo refreshTokenRepo,
    ISessionManager sessionManager,
    IHasher hasher)
{
    public async Task ChangePasswordAsync(ChangePasswordDto dto)
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionsOrRoles(
            IdentityClaims.Types.Identity, IdentityClaims.Values.All,
            [DefaultsRoles.SuperAdmin, DefaultsRoles.Admin]);

        var userId = currentUser.UserId.GetIdAsUlid();

        await using var transaction = await passwordRepo.BeginTransactionAsync();

        var userFromDb = await userRepo.GetUserAsync(userId);

        if (!string.IsNullOrEmpty(userFromDb.PasswordHash))
        {
            if (!hasher.Verify(userFromDb.PasswordHash, dto.OldPassword))
                throw new BadRequestException("Password not correct");
        }

        var passwordHash = hasher.GetHash(dto.NewPassword);

        IdentityPassword activePassword = null;

        if (!options.UserOptions.UseOldPasswords)
        {
            var passwords = await passwordRepo.GetAllUserPasswordsAsync(userId);
            foreach (var password in passwords.Where(s => !s.IsActive))
            {
                if (!hasher.Verify(password.PasswordHash, passwordHash))
                    throw new BadRequestException("Can't be used previews password");
            }
            activePassword = passwords.FirstOrDefault(s => s.IsActive);
        }

        activePassword ??= await passwordRepo.GetUserActivePasswordAsync(userId);

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

        await passwordRepo.AddAsync(newUserPassword);

        if (options.UserOptions.ForceSignoutEverywhere || dto.SignoutEverywhere)
        {
            var userSessions = await sessionRepo.GetActiveSessionsByUserIdAsync(userId);
            var sessionId = currentUser.SessionId.GetIdAsUlid();
            foreach (var session in userSessions)
            {
                session.Status = SessionStatus.Close;
                session.DeactivatedAt = utcNow;
                session.DeactivatedBySessionId = sessionId;
                await sessionRepo.UpdateAsync(session);

                var refreshTokens = await refreshTokenRepo.GetRefreshTokensBySessionIdAsync(session.Id);
                foreach (var refreshToken in refreshTokens)
                {
                    refreshToken.UsedAt = utcNow;
                }
                await refreshTokenRepo.UpdateRangeAsync(refreshTokens);
                await sessionManager.DeleteSessionAsync(session.UserId.GetIdAsString(), sessionId.GetIdAsString());
            }
        }

        await transaction.CommitAsync();
    }
}
