using Extensions.DeviceDetector;
using FluentValidation;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Common.Helpers;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Data.Transactions;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Domain.Enums;
using IdentityPrvd.Features.Authentication.Signin.Dtos;
using IdentityPrvd.Features.Shared.Dtos;
using IdentityPrvd.Features.Shared.Services;
using IdentityPrvd.Mappers;
using IdentityPrvd.Options;
using IdentityPrvd.Services.Location;
using IdentityPrvd.Services.Security;
using IdentityPrvd.Services.ServerSideSessions;

namespace IdentityPrvd.Features.Authentication.Signin.Services;

public class PasswordlessSigninOrchestrator(
    ITokenService tokenService,
    ISessionManager sessionManager,
    IIdentityContext identityContext,
    IUsersQuery usersQuery,
    IClientsQuery clientsQuery,
    ILocationService locationService,
    IDetector detector,
    ITransactionManager transactionManager,
    TimeProvider timeProvider,
    IdentityPrvdOptions identityOptions,
    ISessionStore sessionRepo,
    IMfaStore mfaStore,
    IMfaService mfaService,
    IValidator<PasswordlessSigninRequestDto> validator,
    ISessionControlService sessionControlService)
{
    public async Task<SigninResponseDto> SigninAsync(PasswordlessSigninRequestDto dto)
    {
        await validator.ValidateAndThrowAsync(dto);

        await using var transaction = await transactionManager.BeginTransactionAsync();
        var user = await usersQuery.GetUserByLoginNullableAsync(dto.Login)
            ?? throw new BadRequestException("Login or OTP code is incorrect");
        var client = await clientsQuery.GetClientByIdNullableAsync(dto.ClientId)
            ?? throw new NotFoundException($"Client {dto.ClientId} not found");
        InitClient(dto);
        var location = await locationService.GetIpInfoAsync(identityContext.IpAddress);

        // Check if user has activated MFA
        var userMfa = await mfaStore.GetUserActiveMfaNullableAsync(user.Id);
        if (userMfa == null)
            throw new BadRequestException("MFA is not enabled for this user. Please use regular sign-in.");

        // Verify OTP code
        if (!await mfaService.VerifyMfaAsync(dto.Code, userMfa.Secret))
            throw new BadRequestException("OTP code is invalid");

        // Create session and sign in
        var sessionId = Guid.CreateVersion7();

        var refreshToken = new IdentityRefreshToken
        {
            SessionId = sessionId,
            Value = Generator.GetRefreshToken(),
            ExpiredAt = timeProvider.GetUtcNow().UtcDateTime.AddDays(identityOptions.Token.RefreshLifeTimeInDays)
        };

        var newSession = new IdentitySession
        {
            Id = sessionId,
            UserId = user.Id,
            Client = dto.Client,
            Data = dto.Data,
            App = client.MapToAppInfo(dto.AppVersion),
            Location = location,
            Language = dto.Language,
            Status = SessionStatus.Active,
            Type = SessionType.Mfa,
            ViaMfa = true,
            ExpireAt = timeProvider.GetUtcNow().UtcDateTime.AddDays(identityOptions.Token.SessionLifeTimeInDays),
            Tokens = [refreshToken]
        };

        await sessionRepo.AddAsync(newSession);

        var jwtToken = await tokenService.GetUserTokenAsync(user.Id, sessionId.GetIdAsString(), client.Name);

        var userPermissions = await tokenService.GetUserPermissionsAsync(user.Id, dto.ClientId);

        await sessionManager.AddNewSessionAsync(new SessionInfo
        {
            CreatedAt = newSession.CreatedAt,
            LastAccessedAt = null,
            Permissions = userPermissions,
            SessionExpire = newSession.ExpireAt,
            SessionId = newSession.Id.ToString(),
            UserId = newSession.UserId.ToString(),
        });

        await sessionControlService.CloseOtherSessionsIfRequiredAsync(user.Id, sessionId);

        await transaction.CommitAsync();

        return new SigninResponseDto
        {
            RequiredMfa = false,
            VerifyId = null,
            AccessToken = jwtToken.Token,
            RefreshToken = refreshToken.Value,
            ExpireIn = identityOptions.Token.LifeTimeInMinutes * 60,
        };
    }

    private void InitClient(PasswordlessSigninRequestDto dto)
    {
        if (dto.Client == default)
            dto.Client = detector.GetClientInfo().MapToClientInfo();
    }
}

