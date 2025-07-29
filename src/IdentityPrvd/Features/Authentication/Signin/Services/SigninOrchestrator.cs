using Extensions.DeviceDetector;
using FluentValidation;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Common.Helpers;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Domain.Enums;
using IdentityPrvd.Features.Authentication.Signin.Dtos;
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
    TimeProvider timeProvider,
    TokenOptions tokenOptions,
    ISessionStore sessionRepo,
    IValidator<SigninRequestDto> validator)
{
    public async Task<SigninResponseDto> SigninAsync(SigninRequestDto dto)
    {
        //await using var transaction = await sessionRepo.BeginTransactionAsync();

        await validator.ValidateAndThrowAsync(dto);

        var user = await usersQuery.GetUserByLoginNullableAsync(dto.Login);
        var client = await clientsQuery.GetClientByIdNullableAsync(dto.ClientId);

        if (dto.Client == default)
            dto.Client = detector.GetClientInfo().MapToClientInfo();

        var location = await locationService.GetIpInfoAsync(currentContext.IpAddress);

        var sessionId = Ulid.NewUlid();

        var refreshToken = new IdentityRefreshToken
        {
            SessionId = sessionId,
            Value = Generator.GetRefreshToken(),
            ExpiredAt = timeProvider.GetUtcNow().UtcDateTime.AddDays(tokenOptions.RefreshLifeTimeInDays)
        };

        var newSession = new IdentitySession
        {
            Id = sessionId,
            UserId = user.Id,
            Client = dto.Client,
            App = client.MapToAppInfo(dto.AppVersion),
            Location = location,
            Language = dto.Language,
            Status = SessionStatus.Active,
            Type = SessionType.Password,
            ExpireAt = timeProvider.GetUtcNow().UtcDateTime.AddDays(tokenOptions.SessionLifeTimeInDays),
            ViaMFA = false,
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

        //await transaction.CommitAsync();

        return new SigninResponseDto
        {
            AccessToken = jwtToken.Token,
            RefreshToken = refreshToken.Value,
            ExpiredIn = tokenOptions.LifeTimeInMinutes / 60,
        };
    }
}
