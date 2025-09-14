using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Features.Shared.Dtos;
using IdentityPrvd.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityPrvd.Services.Security;

public class TokenService(
    IdentityPrvdOptions identityOptions,
    TimeProvider timeProvider,
    IUserRolesQuery userRolesQuery,
    IRoleClaimsQuery roleClaimsQuery,
    IClientClaimsQuery clientClaimsQuery,
    IEnumerable<ITokenClaimsContributor> claimsContributors) : ITokenService
{
    public async Task<JwtToken> GetUserTokenAsync(Ulid userId, string sessionId)
    {
        var claims = new List<Claim>
        {
            new(IdentityClaims.Types.UserId, userId.ToString()),
            new(IdentityClaims.Types.SessionId, sessionId),
        };

        var roles = await userRolesQuery.GetUserRoleNamesAsync(userId);

        claims.AddRange(roles.Select(role => new Claim(IdentityClaims.Types.Roles, role)));

        if (claimsContributors.Any())
        {
            var context = new TokenClaimsContext(userId, sessionId, claims);
            foreach (var contributor in claimsContributors)
            {
                await contributor.ContributeAsync(context);
            }
        }

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

        var roles = await userRolesQuery.GetUserRoleNamesAsync(userId);

        claims.AddRange(roles.Select(role => new Claim(IdentityClaims.Types.Roles, role)));

        if (claimsContributors.Any())
        {
            var context = new TokenClaimsContext(userId, sessionId, claims, provider);
            foreach (var contributor in claimsContributors)
            {
                await contributor.ContributeAsync(context);
            }
        }

        return GenerateJwtToken(claims);
    }

    public async Task<Dictionary<string, List<string>>> GetUserPermissionsAsync(Ulid userId, string clientId)
    {
        var roleClaims = await roleClaimsQuery.GetClaimsByUserIdAsync(userId);
        var clientClaims = await clientClaimsQuery.GetClaimsByClientIdAsync(clientId);

        return roleClaims.GroupUnionCollectionBy(clientClaims, c => c.Id, c => c.Type, c => c.Value);
    }

    private JwtToken GenerateJwtToken(List<Claim> claims)
    {
        ClaimsIdentity claimsIdentity = new(claims, JwtBearerDefaults.AuthenticationScheme, ClaimsIdentity.DefaultNameClaimType,
                    ClaimsIdentity.DefaultRoleClaimType);
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        var expiredAt = utcNow.Add(TimeSpan.FromMinutes(identityOptions.Token.LifeTimeInMinutes));
        var jwt = new JwtSecurityToken(
                    issuer: identityOptions.Token.Issuer,
                    audience: identityOptions.Token.Audience,
                    notBefore: utcNow,
                    claims: claimsIdentity.Claims,
                    expires: expiredAt,
                    signingCredentials: new SigningCredentials(
                        new SymmetricSecurityKey(
                            Encoding.ASCII.GetBytes(identityOptions.Token.SecretKey!)), SecurityAlgorithms.HmacSha256));
        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
        return new JwtToken
        {
            Token = encodedJwt,
            ExpiredAt = expiredAt,
            SessionId = claims.FirstOrDefault(s => s.Type == IdentityClaims.Types.SessionId)!.Value
        };
    }
}
