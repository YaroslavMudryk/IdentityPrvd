using IdentityPrvd.WebApi.Db;
using IdentityPrvd.WebApi.Extensions;
using IdentityPrvd.WebApi.Features.Signin.Dtos;
using IdentityPrvd.WebApi.Helpers;
using IdentityPrvd.WebApi.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityPrvd.WebApi.Features.Signin.Services;

public interface ITokenService
{
    Task<JwtToken> GetUserTokenAsync(Ulid userId, string sessionId);
    Task<JwtToken> GetUserTokenAsync(Ulid userId, string sessionId, string provider);
    Task<Dictionary<string, List<string>>> GetUserPermissionsAsync(Ulid userId, string clientId);
}

public class TokenService(
    IdentityPrvdContext dbContext,
    TokenOptions tokenOptions,
    TimeProvider timeProvider) : ITokenService
{
    public async Task<JwtToken> GetUserTokenAsync(Ulid userId, string sessionId)
    {
        var claims = new List<Claim>
        {
            new(IdentityClaims.Types.UserId, userId.ToString()),
            new(IdentityClaims.Types.SessionId, sessionId),
        };

        var roles = await dbContext.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role.Name)
            .ToListAsync();

        claims.AddRange(roles.Select(role => new Claim(IdentityClaims.Types.Roles, role)));

        return GenerateJwtToken(claims);
    }

    public async Task<JwtToken> GetUserTokenAsync(Ulid userId, string sessionId, string provider)
    {
        var claims = new List<Claim>
        {
            new(IdentityClaims.Types.UserId, userId.ToString()),
            new(IdentityClaims.Types.SessionId, sessionId),
            new(IdentityClaims.Types.AuthMethod, provider)
        };

        var roles = await dbContext.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role.Name)
            .ToListAsync();

        claims.AddRange(roles.Select(role => new Claim(IdentityClaims.Types.Roles, role)));

        return GenerateJwtToken(claims);
    }

    public async Task<Dictionary<string, List<string>>> GetUserPermissionsAsync(Ulid userId, string clientId)
    {
        var userRoleIds = await dbContext.UserRoles.AsNoTracking().Where(s => s.UserId == userId).Select(s => s.RoleId).ToListAsync();
        var userClientId = await dbContext.Clients.AsNoTracking().Where(s => s.ClientId == clientId).Select(s => s.Id).FirstOrDefaultAsync();

        var roleClaims = await dbContext.RoleClaims.AsNoTracking().Where(s => userRoleIds.Contains(s.RoleId)).Select(s => s.Claim).ToListAsync();
        var clientClaims = await dbContext.ClientClaims.AsNoTracking().Where(s => s.ClientId == userClientId).Select(s => s.Claim).ToListAsync();

        return roleClaims.GroupUnionCollectionBy(clientClaims, c => c.Id, c => c.Type, c => c.Value);
    }

    private JwtToken GenerateJwtToken(List<Claim> claims)
    {
        ClaimsIdentity claimsIdentity = new(claims, JwtBearerDefaults.AuthenticationScheme, ClaimsIdentity.DefaultNameClaimType,
                    ClaimsIdentity.DefaultRoleClaimType);
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        var expiredAt = utcNow.Add(TimeSpan.FromMinutes(tokenOptions.LifeTimeInMinutes));
        var jwt = new JwtSecurityToken(
                    issuer: tokenOptions.Issuer,
                    audience: tokenOptions.Audience,
                    notBefore: utcNow,
                    claims: claimsIdentity.Claims,
                    expires: expiredAt,
                    signingCredentials: new SigningCredentials(
                        new SymmetricSecurityKey(
                            Encoding.ASCII.GetBytes(tokenOptions.SecretKey!)), SecurityAlgorithms.HmacSha256));
        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
        return new JwtToken
        {
            Token = encodedJwt,
            ExpiredAt = expiredAt,
            SessionId = claims.FirstOrDefault(s => s.Type == IdentityClaims.Types.SessionId)!.Value
        };
    }
}
