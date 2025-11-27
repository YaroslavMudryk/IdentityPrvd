using Extensions.DeviceDetector;
using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Common.Helpers;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Data.Transactions;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Domain.Enums;
using IdentityPrvd.Features.Security.Initialize.Dtos;
using IdentityPrvd.Infrastructure.Caching;
using IdentityPrvd.Infrastructure.Database.Context;
using IdentityPrvd.Infrastructure.Database.Seeding;
using IdentityPrvd.Mappers;
using IdentityPrvd.Options;
using IdentityPrvd.Services.Location;
using IdentityPrvd.Services.Security;
using IdentityPrvd.Services.ServerSideSessions;
using IdentityPrvd.Services.SystemStatus;

namespace IdentityPrvd.Features.Security.Initialize.Services;

public class InitializeOrchestrator(
    IdentityPrvdOptions identityOptions,
    TimeProvider timeProvider,
    IdentityPrvdContext dbContext,
    ITransactionManager transactionManager,
    IDetector detector,
    IIdentityContext identityContext,
    ISystemStatus systemStatus,
    ITokenService tokenService,
    ILocationService locationService,
    ISessionManager sessionManager,
    IPasswordStore passwordStore,
    ISessionStore sessionRepo,
    ISessionManagerStore sessionStore,
    IUserRoleStore userRoleStore,
    IUserStore userStore,
    IRolesQuery rolesQuery,
    IHasher hasher)
{
    public async Task<InitializeResponseDto> InitializeAsync(InitializeRequestDto dto)
    {
        var status = await systemStatus.GetSystemStatusAsync();
        if (status == SystemStatus.ReadyToUse)
            throw new BadRequestException("The system is already initialized.");

        await dbContext.Database.EnsureCreatedAsync();
        await using var transaction = await transactionManager.BeginTransactionAsync();

        await IdentityPrvdSeedHelper.SeedDefaultsAsync(dbContext, hasher, sessionStore, identityOptions.Token.Audience);
        var adminPassword = Generator.GetPassword();
        var user = await InitUserAsync(adminPassword);
        (var refreshToken, var jwtToken) = await InitSessionAsync(dto, user);

        await transaction.CommitAsync();

        return new InitializeResponseDto
        {
            AccessToken = jwtToken.Token,
            RefreshToken = refreshToken.Value,
            Login = user.Login,
            Password = adminPassword
        };
    }

    private async Task<(IdentityRefreshToken refreshToken, Shared.Dtos.JwtToken jwtToken)> InitSessionAsync(InitializeRequestDto dto, IdentityUser user)
    {
        var sessionId = Guid.CreateVersion7();

        var refreshToken = new IdentityRefreshToken
        {
            SessionId = sessionId,
            Value = Generator.GetRefreshToken(),
            ExpiredAt = timeProvider.GetUtcNow().UtcDateTime.AddDays(identityOptions.Token.RefreshLifeTimeInDays)
        };

        var location = await locationService.GetIpInfoAsync(identityContext.IpAddress);
        var newSession = new IdentitySession
        {
            Id = sessionId,
            UserId = user.Id,
            Client = detector.GetClientInfo().MapToClientInfo(),
            App = dbContext.Clients.Local.FirstOrDefault().MapToAppInfo(dto.AppVersion),
            Location = location,
            Language = "en",
            Status = SessionStatus.Active,
            Type = SessionType.Password,
            ExpireAt = timeProvider.GetUtcNow().UtcDateTime.AddDays(identityOptions.Token.SessionLifeTimeInDays),
            ViaMfa = false,
            Tokens = [refreshToken]
        };

        await sessionRepo.AddAsync(newSession);

        var jwtToken = await tokenService.GetUserTokenAsync(user.Id, sessionId.GetIdAsString(), newSession.App.Name);

        var userPermissions = await tokenService.GetUserPermissionsAsync(user.Id, dbContext.Clients.Local.FirstOrDefault().ClientId);

        await sessionManager.AddNewSessionAsync(new SessionInfo
        {
            CreatedAt = newSession.CreatedAt,
            LastAccessedAt = null,
            Permissions = userPermissions,
            SessionExpire = newSession.ExpireAt,
            SessionId = newSession.Id.ToString(),
            UserId = newSession.UserId.ToString(),
        });
        return (refreshToken, jwtToken);
    }

    private async Task<IdentityUser> InitUserAsync(string adminPassword)
    {
        var user = GetUser(adminPassword);
        await userStore.AddAsync(user);

        var roles = await rolesQuery.GetRolesAsync(false);
        var adminRole = roles.First(r => r.Name == DefaultsRoles.Admin);

        var userRole = new IdentityUserRole
        {
            UserId = user.Id,
            RoleId = adminRole.Id
        };
        await userRoleStore.AddAsync(userRole);

        var password = new IdentityPassword
        {
            IsActive = true,
            ActivatedAt = timeProvider.GetUtcNow().DateTime,
            PasswordHash = user.PasswordHash,
            UserId = user.Id,
        };
        await passwordStore.AddAsync(password);
        return user;
    }

    private IdentityUser GetUser(string password)
    {
        return new IdentityUser
        {
            Id = Guid.CreateVersion7(),
            Login = GetLogin(identityOptions.User.LoginType),
            CanBeBlocked = false,
            ConfirmedAt = timeProvider.GetUtcNow().DateTime,
            ConfirmedBy = "system",
            IsConfirmed = true,
            FailedLoginAttemptsCount = 0,
            FirstName = "System",
            LastName = "Administrator",
            MiddleName = string.Empty,
            UserName = "sysadmin",
            Image = "/assets/images/user.png",
            PasswordHash = hasher.GetHash(password)
        };
    }

    private static string GetLogin(LoginType loginType)
    {
        return loginType switch
        {
            LoginType.Email => Generator.GetAdminEmail(),
            LoginType.Phone => Generator.GetAdminPhone(),
            LoginType.Any => Generator.GetString(8),
            _ => throw new ArgumentOutOfRangeException(nameof(loginType), loginType, null)
        };
    }

}
