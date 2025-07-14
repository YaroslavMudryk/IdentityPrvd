using IdentityPrvd.WebApi.CurrentContext;
using IdentityPrvd.WebApi.Db.Entities;
using IdentityPrvd.WebApi.Exceptions;
using IdentityPrvd.WebApi.Extensions;
using IdentityPrvd.WebApi.Features.ExternalSignin.DataAccess;
using IdentityPrvd.WebApi.Features.ExternalSignin.Dtos;
using IdentityPrvd.WebApi.Features.Signin.Dtos;
using IdentityPrvd.WebApi.Features.Signin.Services;
using IdentityPrvd.WebApi.Helpers;
using IdentityPrvd.WebApi.Options;
using IdentityPrvd.WebApi.ServerSideSessions;
using Microsoft.AspNetCore.Authentication;

namespace IdentityPrvd.WebApi.Features.ExternalSignin.Services;

public class ExternalSigninOrchestrator(
    ITokenService tokenService,
    ISessionManager sessionManager,
    TokenOptions tokenOptions,
    ILocationService locationService,
    ICurrentContext currentContext,
    UserRepo userRepo,
    UserLoginRepo userLoginRepo,
    UserRoleRepo userRoleRepo,
    SessionRepo sessionRepo,
    TimeProvider timeProvider,
    ExternalSigninQuery externalSigninQuery,
    ExternalUserExstractorService externalUserExstractor)
{
    public async Task<SigninResponseDto> SigninExternalProviderAsync(AuthenticateResult authResult)
    {
        ValidateAuthenticationResult(authResult);
        
        var dto = authResult.Properties.Items.GetDtoFromItems();
        var user = await externalUserExstractor.GetUserFromExternalProviderAsync(authResult);

        await using var transaction = await userRepo.BeginTransactionAsync();

        var userToLogin = await GetOrCreateUserAsync(user);
        var session = await CreateSessionAsync(dto, userToLogin, user.Provider);
        var jwtToken = await tokenService.GetUserTokenAsync(userToLogin.Id, session.Id.GetIdAsString(), user.Provider);
        var userPermissions = await tokenService.GetUserPermissionsAsync(userToLogin.Id, "1jjd-Pt0B-QFdk-x3Vw");

        await AddSessionToManagerAsync(session, userPermissions);
        await transaction.CommitAsync();

        return CreateSigninResponse(jwtToken, session.Tokens.First());
    }

    private static void ValidateAuthenticationResult(AuthenticateResult authResult)
    {
        if (!authResult.Succeeded)
            throw new BadRequestException("Authentication failed with external provider");
    }

    private async Task<IdentityUser> GetOrCreateUserAsync(ExternalUserDto user)
    {
        var externalLogin = await externalSigninQuery.GetUserLoginByProviderAsync(user.Provider, user.ProviderUserId);

        if (externalLogin == null)
        {
            return await CreateNewUserAsync(user);
        }

        return externalLogin.User;
    }

    private async Task<IdentityUser> CreateNewUserAsync(ExternalUserDto user)
    {
        var defaultRole = await externalSigninQuery.GetDefaultRoleAsync();
        var confirmedAt = timeProvider.GetUtcNow().UtcDateTime;

        var userToLogin = ExternalSigninEntityFactory.CreateIdentityUser(user, confirmedAt, user.Provider);
        var userRole = ExternalSigninEntityFactory.CreateIdentityUserRole(userToLogin.Id, defaultRole.Id);
        var newUserLogin = ExternalSigninEntityFactory.CreateIdentityUserLogin(userToLogin.Id, user.Provider, user.ProviderUserId);

        await userRepo.AddAsync(userToLogin);
        await userRoleRepo.AddAsync(userRole);
        await userLoginRepo.AddAsync(newUserLogin);

        return userToLogin;
    }

    private async Task<IdentitySession> CreateSessionAsync(ExternalSigninDto dto, IdentityUser userToLogin, string provider)
    {
        var sessionId = Ulid.NewUlid();
        var refreshTokenValue = Generator.GetRefreshToken();
        var refreshTokenExpiredAt = timeProvider.GetUtcNow().UtcDateTime.AddDays(tokenOptions.RefreshLifeTimeInDays);
        var sessionExpireAt = timeProvider.GetUtcNow().UtcDateTime.AddDays(tokenOptions.SessionLifeTimeInDays);
        
        var refreshToken = ExternalSigninEntityFactory.CreateRefreshToken(sessionId, refreshTokenValue, refreshTokenExpiredAt);
        var client = await externalSigninQuery.GetClientByIdAsync(dto.ClientId);
        var location = await locationService.GetIpInfoAsync(currentContext.IpAddress);

        var session = ExternalSigninEntityFactory.CreateIdentitySession(sessionId, userToLogin.Id, dto, client, location, refreshToken, sessionExpireAt);

        await sessionRepo.AddAsync(session);
        return session;
    }

    private async Task AddSessionToManagerAsync(IdentitySession session, Dictionary<string, List<string>> userPermissions)
    {
        await sessionManager.AddNewSessionAsync(new SessionInfo
        {
            CreatedAt = session.CreatedAt,
            LastAccessedAt = null,
            Permissions = userPermissions,
            SessionExpire = session.ExpireAt,
            SessionId = session.Id.ToString(),
            UserId = session.UserId.ToString(),
        });
    }

    private SigninResponseDto CreateSigninResponse(JwtToken jwtToken, IdentityRefreshToken refreshToken)
    {
        return new SigninResponseDto
        {
            AccessToken = jwtToken.Token,
            RefreshToken = refreshToken.Value,
            ExpiredIn = tokenOptions.LifeTimeInMinutes / 60,
        };
    }
}
