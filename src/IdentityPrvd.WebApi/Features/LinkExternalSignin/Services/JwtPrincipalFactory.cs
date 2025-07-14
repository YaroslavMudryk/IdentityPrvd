using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityPrvd.WebApi.Features.LinkExternalSignin.Services;

public class JwtPrincipalFactory
{
    public static ClaimsPrincipal? CreatePrincipalFromJwt(string jwtToken, IConfiguration configuration)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(configuration["Token:SecretKey"]!); // Ваш секретний ключ
        var issuer = configuration["Token:Issuer"];
        var audience = configuration["Token:Audience"];

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero // Важливо для точного часу дії токену
        };

        try
        {
            // Валідуємо токен і отримуємо Principal
            SecurityToken validatedToken;
            var principal = tokenHandler.ValidateToken(jwtToken, tokenValidationParameters, out validatedToken);

            // Якщо токен валідний, повертаємо Principal
            return principal;
        }
        catch (SecurityTokenExpiredException)
        {
            // Токен закінчив термін дії
            Console.WriteLine("JWT Token has expired.");
            return null;
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            // Неправильний підпис
            Console.WriteLine("JWT Token has invalid signature.");
            return null;
        }
        catch (Exception ex)
        {
            // Інші помилки валідації
            Console.WriteLine($"JWT Token validation failed: {ex.Message}");
            return null;
        }
    }
}
