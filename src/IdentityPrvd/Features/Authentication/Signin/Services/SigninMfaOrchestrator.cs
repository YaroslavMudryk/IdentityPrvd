using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Common.Helpers;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Data.Transactions;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Domain.Enums;
using IdentityPrvd.Features.Authentication.Signin.Dtos;
using IdentityPrvd.Features.Shared.Dtos;
using IdentityPrvd.Options;
using IdentityPrvd.Services.Security;
using IdentityPrvd.Services.ServerSideSessions;

namespace IdentityPrvd.Features.Authentication.Signin.Services;

public class SigninMfaOrchestrator(
    ISessionStore sessionStore,
    IRefreshTokenStore refreshTokenStore,
    IMfaStore mfaStore,
    IMfaService mfaService,
    TimeProvider timeProvider,
    ITokenService tokenService,
    ISessionManager sessionManager,
    IdentityPrvdOptions identityOptions,
    ITransactionManager transactionManager)
{
    public async Task<SigninResponseDto> SinginMfaAsync(SigninMfaRequestDto dto)
    {
        await using var transaction = await transactionManager.BeginTransactionAsync();

        var sessionToActivate = await sessionStore.GetSessionByVerificationIdAsync(dto.VerificationId) ?? throw new BadRequestException("VerificationId is not valid");
        if (sessionToActivate.VerificationExpire < timeProvider.GetUtcNow().UtcDateTime)
        {
            throw new BadRequestException("Verification id has expired");
        }

        var mfa = await mfaStore.GetByUserIdAsync(sessionToActivate.UserId);

        if (!await mfaService.VerifyMfaAsync(dto.Code, mfa.Secret))
            throw new BadRequestException("Your otp code is invalid");

        sessionToActivate.Status = SessionStatus.Active;
        sessionToActivate.VerificationId = null;
        sessionToActivate.VerificationExpire = null;
        await sessionStore.UpdateAsync(sessionToActivate);

        var refreshToken = new IdentityRefreshToken
        {
            SessionId = sessionToActivate.Id,
            Value = Generator.GetRefreshToken(),
            ExpiredAt = timeProvider.GetUtcNow().UtcDateTime.AddDays(identityOptions.Token.RefreshLifeTimeInDays)
        };
        await refreshTokenStore.AddAsync(refreshToken);

        var userToken = await tokenService.GetUserTokenAsync(sessionToActivate.UserId, sessionToActivate.Id.GetIdAsString());

        var userPermissions = await tokenService.GetUserPermissionsAsync(sessionToActivate.UserId, sessionToActivate.App.Id.GetIdAsString());

        await sessionManager.AddNewSessionAsync(new SessionInfo
        {
            CreatedAt = sessionToActivate.CreatedAt,
            LastAccessedAt = null,
            Permissions = userPermissions,
            SessionExpire = sessionToActivate.ExpireAt,
            SessionId = sessionToActivate.Id.ToString(),
            UserId = sessionToActivate.UserId.ToString(),
        });

        await transaction.CommitAsync();

        return new SigninResponseDto
        {
            AccessToken = userToken.Token,
            RefreshToken = refreshToken.Value,
            ExpiredIn = identityOptions.Token.LifeTimeInMinutes / 60,
        };
    }
}
