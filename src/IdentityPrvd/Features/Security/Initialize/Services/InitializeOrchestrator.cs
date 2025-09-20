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
using IdentityPrvd.Mappers;
using IdentityPrvd.Options;
using IdentityPrvd.Services.Location;
using IdentityPrvd.Services.Security;
using IdentityPrvd.Services.ServerSideSessions;
using IdentityPrvd.Services.SystemStatus;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Features.Security.Initialize.Services;

public class InitializeOrchestrator(
    IdentityPrvdOptions identityOptions,
    TimeProvider timeProvider,
    IdentityPrvdContext dbContext,
    ITransactionManager transactionManager,
    IDetector detector,
    ICurrentContext currentContext,
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

        await InitDbAsync();
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
        var sessionId = Ulid.NewUlid();

        var refreshToken = new IdentityRefreshToken
        {
            SessionId = sessionId,
            Value = Generator.GetRefreshToken(),
            ExpiredAt = timeProvider.GetUtcNow().UtcDateTime.AddDays(identityOptions.Token.RefreshLifeTimeInDays)
        };

        var location = await locationService.GetIpInfoAsync(currentContext.IpAddress);
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

        var jwtToken = await tokenService.GetUserTokenAsync(user.Id, sessionId.GetIdAsString());

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
        var superAdminRole = roles.First(r => r.Name == DefaultsRoles.SuperAdmin);

        var userRole = new IdentityUserRole
        {
            UserId = user.Id,
            RoleId = superAdminRole.Id
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

    private async Task InitDbAsync()
    {
        var itemsCountAdded = 0;
        if (!await dbContext.Roles.AnyAsync())
        {
            await dbContext.Roles.AddRangeAsync(SeedConstants.GetRoles());
            itemsCountAdded++;
        }
        if (!await dbContext.Claims.AnyAsync())
        {
            await dbContext.Claims.AddRangeAsync(SeedConstants.GetClaims());
            itemsCountAdded++;
        }
        if (!await dbContext.Clients.AnyAsync())
        {
            await dbContext.Clients.AddRangeAsync(SeedConstants.GetClients(hasher));
            itemsCountAdded++;
        }
        if (itemsCountAdded > 0)
        {
            await dbContext.SaveChangesAsync();
            await MapRolesAndClaimsAsync(dbContext);
        }
        await sessionStore.InitializeAsync();
    }

    private IdentityUser GetUser(string password)
    {
        return new IdentityUser
        {
            Id = Ulid.NewUlid(),
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

    private static async Task MapRolesAndClaimsAsync(IdentityPrvdContext dbContext)
    {
        var defaultRole = dbContext.Roles.Local.Where(r => r.IsDefault).FirstOrDefault();
        var identityClaim = dbContext.Claims.Local.FirstOrDefault(c => c.Type == IdentityClaims.Types.Identity && c.Value == IdentityClaims.Values.All);

        await dbContext.RoleClaims.AddAsync(new IdentityRoleClaim
        {
            Id = Ulid.NewUlid(),
            RoleId = defaultRole!.Id,
            ClaimId = identityClaim!.Id,
            ActiveFrom = DateTime.MinValue,
            ActiveTo = DateTime.MaxValue,
            IsActive = true
        });
        await dbContext.SaveChangesAsync();
    }
}
