using IdentityPrvd.Common.Extensions;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Redis.OM;
using Redis.OM.Contracts;
using System.Text;

namespace IdentityPrvd;

public static class ServicesCollectionExtensions
{
    public static IServiceCollection AddIdentityPrvd(this IServiceCollection services, IConfiguration configuration, Action<IdentityPrvdOptions> actionOptions)
    {
        var options = new IdentityPrvdOptions();
        actionOptions.Invoke(options);
        options.ValidateAndThrowIfNeeded();
        services.AddScoped<IdentityPrvdOptions>((sp) => options);
        var identityPrvdSection = configuration.GetSection("IdentityPrvd");

        services.AddEndpoints();

        //db
        services.AddDbContext<IdentityPrvdContext>(options =>
            options.UseNpgsql(identityPrvdSection["Connections:Db"])
                .UseSnakeCaseNamingConvention());
        services.AddScoped<IRedisConnectionProvider>(_ =>
            new RedisConnectionProvider(identityPrvdSection["Connections:Redis"]));

        services.AddScoped<ITransactionManager, EfCoreTransactionManager>();
        //services.AddScoped<ITransactionScope, EfCoreTransactionScope>();



        //middlewares
        services.AddTransient<CorrelationContextMiddleware>();
        services.AddTransient<ServerSideSessionMiddleware>();
        services.AddTransient<GlobalExceptionHandlerMiddleware>();

        //features
        services.AddSignupDependencies();
        services.AddSigninDependencies();
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

        //protection
        services.AddScopedConfiguration<AppOptions>(identityPrvdSection, "App");
        services.AddScopedConfiguration<ProtectionOptions>(identityPrvdSection, "Protection");
        services.AddScoped<IProtectionService, AesProtectionService>();
        services.AddScoped<IMfaService, TotpMfaService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IHasher, Sha512Hasher>();


        //others
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<ICurrentContext, CurrentContext>();
        services.AddScoped<ISessionManager, SessionManager>();
        services.AddScoped<Infrastructure.Caching.ISessionStore, RedisSessionStore>();
        services.AddScopedConfiguration<TokenOptions>(identityPrvdSection, "Token");
        services.AddScoped<UserHelper>();
        services.AddHttpClient<ILocationService, IpApiLocationService>("Location", options =>
        {
            options.BaseAddress = new Uri("http://ip-api.com");
        });

        services.AddStoresAndQueries();

        services.AddScoped<IEmailService, FakeEmailService>();
        services.AddScoped<ISmsService, FakeSmsService>();


        services.AddSingleton(TimeProvider.System);
        services.AddAuthorization();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddCookie("cookie")
            .AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = identityPrvdSection["Auth:Google:ClientId"];
                googleOptions.ClientSecret = identityPrvdSection["Auth:Google:ClientSecret"];
                googleOptions.CallbackPath = "/signin-google";
                googleOptions.SignInScheme = "cookie";
                googleOptions.SaveTokens = true;
            })
            .AddMicrosoftAccount(microsoftOptions =>
            {
                microsoftOptions.ClientId = identityPrvdSection["Auth:Microsoft:ClientId"];
                microsoftOptions.ClientSecret = identityPrvdSection["Auth:Microsoft:ClientSecret"];
                microsoftOptions.CallbackPath = "/signin-microsoft";
                microsoftOptions.SignInScheme = "cookie";
                microsoftOptions.SaveTokens = true;
            })
            .AddGitHub(githubOptions =>
            {
                githubOptions.ClientId = identityPrvdSection["Auth:GitHub:ClientId"];
                githubOptions.ClientSecret = identityPrvdSection["Auth:GitHub:ClientSecret"];
                githubOptions.CallbackPath = "/signin-github";
                githubOptions.SignInScheme = "cookie";
                githubOptions.Scope.Add("read:user");
                githubOptions.Scope.Add("user:email");
                githubOptions.SaveTokens = true;
            })
            .AddFacebook(facebookOptions =>
            {
                facebookOptions.AppId = identityPrvdSection["Auth:Facebook:AppId"];
                facebookOptions.AppSecret = identityPrvdSection["Auth:Facebook:AppSecret"];
                facebookOptions.CallbackPath = "/signin-facebook";
                facebookOptions.SignInScheme = "cookie";
                facebookOptions.SaveTokens = true;
            })
            .AddTwitter(twitterOptions =>
            {
                twitterOptions.ClientId = identityPrvdSection["Auth:Twitter:ClientId"];
                twitterOptions.ClientSecret = identityPrvdSection["Auth:Twitter:ClientSecret"];
                twitterOptions.CallbackPath = "/signin-twitter";
                twitterOptions.SignInScheme = "cookie";
                twitterOptions.Scope.Add("users.read");
                twitterOptions.Scope.Add("users.email");
                twitterOptions.SaveTokens = true;
            })
            .AddJwtBearer(jwt =>
            {
                jwt.RequireHttpsMetadata = false;
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ClockSkew = TimeSpan.Zero,
                    ValidateIssuer = true,
                    ValidIssuer = identityPrvdSection["Token:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = identityPrvdSection["Token:Audience"],
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(identityPrvdSection["Token:SecretKey"]!)),
                    ValidateIssuerSigningKey = true,
                };
                jwt.SaveToken = true;
            });


        return services;
    }

    private static IServiceCollection AddStoresAndQueries(this IServiceCollection services)
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
