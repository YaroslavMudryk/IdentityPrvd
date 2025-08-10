using IdentityPrvd.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace IdentityPrvd.Extensions.Old;

/// <summary>
/// Builder for authentication and authorization
/// </summary>
public class IdentityAuthenticationBuilder(IServiceCollection services, IdentityPrvdOptions options)
{
    private readonly Microsoft.AspNetCore.Authentication.AuthenticationBuilder _authBuilder = services.AddAuthentication();

    public Microsoft.AspNetCore.Authentication.AuthenticationBuilder AuthenticationBuilder => _authBuilder;

    /// <summary>
    /// Add JWT Bearer authentication
    /// </summary>
    public IdentityAuthenticationBuilder AddJwtBearer()
    {
        _authBuilder
            .AddCookie("cookie")
            .AddJwtBearer(jwt =>
            {
                jwt.RequireHttpsMetadata = false;
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ClockSkew = TimeSpan.Zero,
                    ValidateIssuer = true,
                    ValidIssuer = options.Token.Issuer,
                    ValidateAudience = true,
                    ValidAudience = options.Token.Audience,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(options.Token.SecretKey!)),
                    ValidateIssuerSigningKey = true,
                };
                jwt.SaveToken = true;
            });

        return this;
    }

    /// <summary>
    /// Add authorization
    /// </summary>
    public IdentityAuthenticationBuilder AddAuthorization()
    {
        services.AddAuthorization();
        return this;
    }

    /// <summary>
    /// Add all authentication services
    /// </summary>
    public IdentityAuthenticationBuilder AddAll()
    {
        return AddAuthorization().AddJwtBearer();
    }
} 