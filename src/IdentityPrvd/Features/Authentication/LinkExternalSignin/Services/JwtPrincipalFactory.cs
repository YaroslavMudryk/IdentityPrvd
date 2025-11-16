using IdentityPrvd.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityPrvd.Features.Authentication.LinkExternalSignin.Services;

public class JwtPrincipalFactory
{
    public static ClaimsPrincipal CreatePrincipalFromJwt(string jwtToken, IdentityPrvdOptions options)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(options.Token.SecretKey!);
        var issuer = options.Token.Issuer;

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateLifetime = true,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var principal = tokenHandler.ValidateToken(jwtToken, tokenValidationParameters, out SecurityToken validatedToken);

            return principal;
        }
        catch (SecurityTokenExpiredException)
        {
            Console.WriteLine("JWT Token has expired.");
            return null;
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            Console.WriteLine("JWT Token has invalid signature.");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JWT Token validation failed: {ex.Message}");
            return null;
        }
    }
}
