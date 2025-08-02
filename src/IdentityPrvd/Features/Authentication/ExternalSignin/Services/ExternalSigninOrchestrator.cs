using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Common.Helpers;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Data.Transactions;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Features.Authentication.ExternalSignin.Dtos;
using IdentityPrvd.Features.Shared.Dtos;
using IdentityPrvd.Options;
using IdentityPrvd.Services.Location;
using IdentityPrvd.Services.Security;
using IdentityPrvd.Services.ServerSideSessions;
using Microsoft.AspNetCore.Authentication;

namespace IdentityPrvd.Features.Authentication.ExternalSignin.Services;

public class ExternalSigninOrchestrator(
    ITokenService tokenService,
    ISessionManager sessionManager,
    IdentityPrvdOptions identityOptions,
    ILocationService locationService,
    ICurrentContext currentContext,
    IRolesQuery rolesQuery,
    IUserStore userRepo,
    IClientsQuery clientsQuery,
    IUserLoginStore userLoginRepo,
    IUserRoleStore userRoleRepo,
    ISessionStore sessionRepo,
    ITransactionManager transactionManager,
    TimeProvider timeProvider,
    IUserLoginsQuery externalSigninQuery,
    ExternalUserExstractorService externalUserExstractor)
{
    public async Task<SigninResponseDto> SigninExternalProviderAsync(AuthenticateResult authResult)
    {
        ValidateAuthenticationResult(authResult);
        
        var dto = authResult.Properties.Items.GetDtoFromItems();
        var user = await externalUserExstractor.GetUserFromExternalProviderAsync(authResult);

        await using var transaction = await transactionManager.BeginTransactionAsync();

        var userToLogin = await GetOrCreateUserAsync(user);
        var session = await CreateSessionAsync(dto, userToLogin, user.Provider);
        var jwtToken = await tokenService.GetUserTokenAsync(userToLogin.Id, session.Id.GetIdAsString(), user.Provider);
        var userPermissions = await tokenService.GetUserPermissionsAsync(userToLogin.Id, dto.ClientId);

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
        var externalLogin = await externalSigninQuery.GetUserLoginByProviderWithUserAsync(user.ProviderUserId, user.Provider);

        if (externalLogin == null)
        {
            return await CreateNewUserAsync(user);
        }

        return externalLogin.User;
    }

    private async Task<IdentityUser> CreateNewUserAsync(ExternalUserDto user)
    {
        var defaultRole = await rolesQuery.GetDefaultRoleAsync();
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
        var refreshTokenExpiredAt = timeProvider.GetUtcNow().UtcDateTime.AddDays(identityOptions.Token.RefreshLifeTimeInDays);
        var sessionExpireAt = timeProvider.GetUtcNow().UtcDateTime.AddDays(identityOptions.Token.SessionLifeTimeInDays);
        
        var refreshToken = ExternalSigninEntityFactory.CreateRefreshToken(sessionId, refreshTokenValue, refreshTokenExpiredAt);
        var client = await clientsQuery.GetClientByIdAsync(dto.ClientId);
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
            ExpiredIn = identityOptions.Token.LifeTimeInMinutes / 60,
        };
    }
}
