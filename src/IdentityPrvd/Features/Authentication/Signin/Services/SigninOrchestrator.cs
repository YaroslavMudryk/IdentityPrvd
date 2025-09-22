using Extensions.DeviceDetector;
using FluentValidation;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Common.Helpers;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Data.Transactions;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Domain.Enums;
using IdentityPrvd.Domain.ValueObjects;
using IdentityPrvd.Features.Authentication.Signin.Dtos;
using IdentityPrvd.Features.Shared.Dtos;
using IdentityPrvd.Mappers;
using IdentityPrvd.Options;
using IdentityPrvd.Services.Location;
using IdentityPrvd.Services.Security;
using IdentityPrvd.Services.ServerSideSessions;

namespace IdentityPrvd.Features.Authentication.Signin.Services;

public class SigninOrchestrator(
    ITokenService tokenService,
    ISessionManager sessionManager,
    ICurrentContext currentContext,
    IUsersQuery usersQuery,
    IClientsQuery clientsQuery,
    ILocationService locationService,
    IDetector detector,
    ITransactionManager transactionManager,
    TimeProvider timeProvider,
    IdentityPrvdOptions identityOptions,
    ISessionStore sessionRepo,
    IMfasQuery mfasQuery,
    IValidator<SigninRequestDto> validator)
{
    public async Task<SigninResponseDto> SigninAsync(SigninRequestDto dto)
    {
        await validator.ValidateAndThrowAsync(dto);

        await using var transaction = await transactionManager.BeginTransactionAsync();
        var user = await usersQuery.GetUserByLoginNullableAsync(dto.Login);
        var client = await clientsQuery.GetClientByIdNullableAsync(dto.ClientId);        
        InitClient(dto);
        var location = await locationService.GetIpInfoAsync(currentContext.IpAddress);

        var response = new SigninResponseDto();

        var userMfa = await mfasQuery.GetMfaByUserIdAsync(user.Id);
        if (userMfa == null)
            response = await SigninAsync(user, dto, client, location);
        else
            response = await SigninMfaAsync(user, dto, client, location);

        await transaction.CommitAsync();

        return response;
    }

    private void InitClient(SigninRequestDto dto)
    {
        if (dto.Client == default)
            dto.Client = detector.GetClientInfo().MapToClientInfo();
    }

    private async Task<SigninResponseDto> SigninMfaAsync(IdentityUser user, SigninRequestDto dto, IdentityClient client, LocationInfo location)
    {
        var sessionVerification = Generator.GetSessionVerification();
        var sessionId = Ulid.NewUlid();

        var newSession = new IdentitySession
        {
            Id = sessionId,
            UserId = user.Id,
            Client = dto.Client,
            Data = dto.Data,
            App = client.MapToAppInfo(dto.AppVersion),
            Location = location,
            Language = dto.Language,
            Status = SessionStatus.New,
            Type = SessionType.Mfa,
            ViaMfa = true,
            ExpireAt = timeProvider.GetUtcNow().UtcDateTime.AddDays(identityOptions.Token.SessionLifeTimeInDays),
            VerificationId = sessionVerification,
            VerificationExpire = timeProvider.GetUtcNow().UtcDateTime.AddHours(1)
        };
        await sessionRepo.AddAsync(newSession);

        return new SigninResponseDto
        {
            RequiredMfa = true,
            VerifyId = sessionVerification
        };
    }

    private async Task<SigninResponseDto> SigninAsync(IdentityUser user, SigninRequestDto dto, IdentityClient client, LocationInfo location)
    {
        var sessionId = Ulid.NewUlid();

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
            Type = SessionType.Password,
            ExpireAt = timeProvider.GetUtcNow().UtcDateTime.AddDays(identityOptions.Token.SessionLifeTimeInDays),
            ViaMfa = false,
            Tokens = [refreshToken]
        };

        await sessionRepo.AddAsync(newSession);

        var jwtToken = await tokenService.GetUserTokenAsync(user.Id, sessionId.GetIdAsString());

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

        return new SigninResponseDto
        {
            RequiredMfa = false,
            VerifyId = null,
            AccessToken = jwtToken.Token,
            RefreshToken = refreshToken.Value,
            ExpireIn = identityOptions.Token.LifeTimeInMinutes * 60,
        };
    }
}
