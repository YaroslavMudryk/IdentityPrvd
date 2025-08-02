using IdentityPrvd.Common.Helpers;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Data.Transactions;
using IdentityPrvd.Endpoints;
using IdentityPrvd.Features.Authentication.ChangeLogin;
using IdentityPrvd.Features.Authentication.ChangePassword;
using IdentityPrvd.Features.Authentication.ExternalSignin;
using IdentityPrvd.Features.Authentication.LinkExternalSignin;
using IdentityPrvd.Features.Authentication.RestorePassword;
using IdentityPrvd.Features.Authentication.Signin;
using IdentityPrvd.Features.Authentication.SigninOptions;
using IdentityPrvd.Features.Authentication.Signout;
using IdentityPrvd.Features.Authentication.Signup;
using IdentityPrvd.Features.Authorization.Claims;
using IdentityPrvd.Features.Authorization.Roles;
using IdentityPrvd.Features.Personal.Contacts;
using IdentityPrvd.Features.Personal.Devices;
using IdentityPrvd.Features.Security.Mfa.DisableMfa;
using IdentityPrvd.Features.Security.Mfa.EnableMfa;
using IdentityPrvd.Features.Security.RefreshToken;
using IdentityPrvd.Features.Security.Sessions.GetSessions;
using IdentityPrvd.Features.Security.Sessions.RevokeSessions;
using IdentityPrvd.Infrastructure.Caching;
using IdentityPrvd.Infrastructure.Database.Context;
using IdentityPrvd.Infrastructure.Database.Transactions;
using IdentityPrvd.Infrastructure.Middleware;
using IdentityPrvd.Options;
using IdentityPrvd.Services.Location;
using IdentityPrvd.Services.Notification;
using IdentityPrvd.Services.Security;
using IdentityPrvd.Services.ServerSideSessions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Redis.OM;
using Redis.OM.Contracts;
using System.Text;

namespace IdentityPrvd;

public static class ServicesCollectionExtensions
{
    public static IServiceCollection AddIdentityPrvd(this IServiceCollection services)
    {
        var identityOptions = new IdentityPrvdOptions();
        return services.AddIdentityPrvd(identityOptions);
    }

    public static IServiceCollection AddIdentityPrvd(this IServiceCollection services, Action<IdentityPrvdOptions> actionOptions)
    {
        var identityOptions = new IdentityPrvdOptions();
        actionOptions.Invoke(identityOptions);
        return services.AddIdentityPrvd(identityOptions);
    }

    public static IServiceCollection AddIdentityPrvd(this IServiceCollection services, IConfiguration configuration)
    {
        var identityOptions = configuration.GetSection("IdentityPrvd").Get<IdentityPrvdOptions>();
        return services.AddIdentityPrvd(identityOptions);
    }

    public static IServiceCollection AddIdentityPrvd(this IServiceCollection services, IConfiguration configuration, Action<IdentityPrvdOptions> actionOptions)
    {
        var identityOptions = configuration.GetSection("IdentityPrvd").Get<IdentityPrvdOptions>();
        actionOptions.Invoke(identityOptions);
        return services.AddIdentityPrvd(identityOptions);
    }

    public static IServiceCollection AddIdentityPrvd(this IServiceCollection services, IdentityPrvdOptions identityOptions)
    {
        identityOptions.ValidateAndThrowIfNeeded();
        services.AddScoped(_ => identityOptions);

        services.AddEndpoints();

        services.AddDbServices(identityOptions);

        services.AddMiddlewares();

        services.AddFeatures();

        services.AddProtectionServices();

        services.AddContexts();

        services.AddServerSideSessions();

        services.AddIdentityAuth(identityOptions);

        services.AddOthers();

        return services;
    }

    private static IServiceCollection AddDbServices(this IServiceCollection services, IdentityPrvdOptions identityOptions)
    {
        services.AddScoped<IClaimsQuery, EfClaimsQuery>();
        services.AddScoped<IClientsQuery, EfClientsQuery>();
        services.AddScoped<IRefreshTokensQuery, EfRefreshTokensQuery>();
        services.AddScoped<IRolesQuery, EfRolesQuery>();
        services.AddScoped<ISessionsQuery, EfSessionsQuery>();
        services.AddScoped<IUserLoginsQuery, EfUserLoginsQuery>();
        services.AddScoped<IUsersQuery, EfUsersQuery>();
        services.AddScoped<IContactsQuery, EfContactsQuery>();
        services.AddScoped<IDevicesQuery, EfDevicesQuery>();

        services.AddScoped<IClaimStore, EfClaimStore>();
        services.AddScoped<IConfirmStore, EfConfirmStore>();
        services.AddScoped<IMfaStore, EfMfaStore>();
        services.AddScoped<IPasswordStore, EfPasswordStore>();
        services.AddScoped<IRefreshTokenStore, EfRefreshTokenStore>();
        services.AddScoped<IRoleClaimStore, EfRoleClaimStore>();
        services.AddScoped<IRoleStore, EfRoleStore>();
        services.AddScoped<Data.Stores.ISessionStore, EfSessionStore>();
        services.AddScoped<IUserLoginStore, EfUserLoginStore>();
        services.AddScoped<IUserRoleStore, EfUserRoleStore>();
        services.AddScoped<IUserStore, EfUserStore>();
        services.AddScoped<IContactStore, EfContactStore>();
        services.AddScoped<IDeviceStore, EfDeviceStore>();

        services.AddDbContext<IdentityPrvdContext>(options =>
            options.UseNpgsql(identityOptions.Connections.Db) //Postgres db
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IRedisConnectionProvider>(_ =>
            new RedisConnectionProvider(identityOptions.Connections.Redis));

        services.AddScoped<ITransactionManager, EfCoreTransactionManager>();

        return services;
    }

    private static IServiceCollection AddProtectionServices(this IServiceCollection services)
    {
        services.AddScoped<IProtectionService, AesProtectionService>();
        services.AddScoped<IMfaService, TotpMfaService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IHasher, Sha512Hasher>();
        return services;
    }

    private static IServiceCollection AddFeatures(this IServiceCollection services)
    {
        services.AddSignupDependencies();
        services.AddSigninDependencies();
        services.AddSigninOptionsDependencies();
        services.AddRefreshTokenDependencies();
        services.AddSignoutDependencies();
        services.AddGetSessionsDependencies();
        services.AddRevokeSessionsDependencies();
        services.AddEnableMfaDependencies();
        services.AddDisableMfaDependencies();
        services.AddRolesDependencies();
        services.AddClaimsDependencies();
        services.AddExternalSigninDependencies();
        services.AddLinkExternalProviderDependencies();
        services.AddChangeLoginDependencies();
        services.AddChangePasswordDependencies();
        services.AddRestorePasswordDependencies();
        services.AddContactsDependencies();
        services.AddDevicesDependencies();
        return services;
    }

    private static IServiceCollection AddMiddlewares(this IServiceCollection services)
    {
        services.AddTransient<CorrelationContextMiddleware>();
        services.AddTransient<ServerSideSessionMiddleware>();
        services.AddTransient<GlobalExceptionHandlerMiddleware>();
        return services;
    }

    private static IServiceCollection AddContexts(this IServiceCollection services)
    {
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<ICurrentContext, CurrentContext>();
        return services;
    }

    private static IServiceCollection AddServerSideSessions(this IServiceCollection services)
    {
        services.AddScoped<ISessionManager, SessionManager>();
        services.AddScoped<Infrastructure.Caching.ISessionStore, RedisSessionStore>();
        services.AddTransient<ServerSideSessionMiddleware>();
        return services;
    }

    private static IServiceCollection AddIdentityAuth(this IServiceCollection services, IdentityPrvdOptions identityOptions)
    {
        services.AddAuthorization();
        var identityBuilder = services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddCookie("cookie");

        if (identityOptions.ExternalProviders["Google"].IsAvailable)
        {
            identityBuilder.AddGoogle(options =>
            {
                options.ClientId = identityOptions.ExternalProviders["Google"].ClientId;
                options.ClientSecret = identityOptions.ExternalProviders["Google"].ClientSecret;
                options.CallbackPath = "/signin-google";
                options.SignInScheme = "cookie";
                options.SaveTokens = true;
            });
        }
        if (identityOptions.ExternalProviders["Microsoft"].IsAvailable)
        {
            identityBuilder.AddMicrosoftAccount(options =>
            {
                options.ClientId = identityOptions.ExternalProviders["Microsoft"].ClientId;
                options.ClientSecret = identityOptions.ExternalProviders["Microsoft"].ClientSecret;
                options.CallbackPath = "/signin-microsoft";
                options.SignInScheme = "cookie";
                options.SaveTokens = true;
            });
        }
        if (identityOptions.ExternalProviders["GitHub"].IsAvailable)
        {
            identityBuilder.AddGitHub(options =>
            {
                options.ClientId = identityOptions.ExternalProviders["GitHub"].ClientId;
                options.ClientSecret = identityOptions.ExternalProviders["GitHub"].ClientSecret;
                options.CallbackPath = "/signin-github";
                options.SignInScheme = "cookie";
                options.Scope.Add("read:user");
                options.Scope.Add("user:email");
                options.SaveTokens = true;
            });
        }
        if (identityOptions.ExternalProviders["Facebook"].IsAvailable)
        {
            identityBuilder.AddFacebook(options =>
            {
                options.AppId = identityOptions.ExternalProviders["Facebook"].ClientId;
                options.AppSecret = identityOptions.ExternalProviders["Facebook"].ClientSecret;
                options.CallbackPath = "/signin-facebook";
                options.SignInScheme = "cookie";
                options.SaveTokens = true;
            });
        }
        if (identityOptions.ExternalProviders["Twitter"].IsAvailable)
        {
            identityBuilder.AddTwitter(options =>
            {
                options.ClientId = identityOptions.ExternalProviders["Twitter"].ClientId;
                options.ClientSecret = identityOptions.ExternalProviders["Twitter"].ClientSecret;
                options.CallbackPath = "/signin-twitter";
                options.SignInScheme = "cookie";
                options.Scope.Add("users.read");
                options.Scope.Add("users.email");
                options.SaveTokens = true;
            });
        }
        if (identityOptions.ExternalProviders["Steam"].IsAvailable)
        {
            identityBuilder.AddSteam(options =>
            {
                options.ApplicationKey = identityOptions.ExternalProviders["Steam"].ClientId;
                options.CallbackPath = "/signin-steam";
                options.SignInScheme = "cookie";
                options.SaveTokens = true;
            });
        }
        identityBuilder
            .AddJwtBearer(jwt =>
            {
                jwt.RequireHttpsMetadata = false;
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ClockSkew = TimeSpan.Zero,
                    ValidateIssuer = true,
                    ValidIssuer = identityOptions.Token.Issuer,
                    ValidateAudience = true,
                    ValidAudience = identityOptions.Token.Audience,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(identityOptions.Token.SecretKey!)),
                    ValidateIssuerSigningKey = true,
                };
                jwt.SaveToken = true;
            });

        return services;
    }

    private static IServiceCollection AddOthers(this IServiceCollection services)
    {
        services.AddScoped<UserHelper>();
        services.AddHttpClient<ILocationService, IpApiLocationService>("Location", options =>
        {
            options.BaseAddress = new Uri("http://ip-api.com");
        });

        services.AddScoped<IEmailService, FakeEmailService>();
        services.AddScoped<ISmsService, FakeSmsService>();
        services.TryAddSingleton(TimeProvider.System);

        return services;
    }
}

public static class WebApplicationExtensions
{
    public static WebApplication UseIdentityPrvd(this WebApplication app)
    {
        app.UseMiddleware<CorrelationContextMiddleware>();
        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseServerSideSessions();

        app.MapEndpoints();
        return app;
    }
}
