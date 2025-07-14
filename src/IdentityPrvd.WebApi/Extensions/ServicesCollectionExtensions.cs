using IdentityPrvd.WebApi.CurrentContext;
using IdentityPrvd.WebApi.Db;
using IdentityPrvd.WebApi.EmailSender;
using IdentityPrvd.WebApi.Features;
using IdentityPrvd.WebApi.Features.ChangeLogin;
using IdentityPrvd.WebApi.Features.ChangePassword;
using IdentityPrvd.WebApi.Features.Claims;
using IdentityPrvd.WebApi.Features.DisableMfa;
using IdentityPrvd.WebApi.Features.EnableMfa;
using IdentityPrvd.WebApi.Features.ExternalSignin;
using IdentityPrvd.WebApi.Features.LinkExternalSignin;
using IdentityPrvd.WebApi.Features.RefreshToken;
using IdentityPrvd.WebApi.Features.RestorePassword;
using IdentityPrvd.WebApi.Features.Roles;
using IdentityPrvd.WebApi.Features.Sessions.GetSessions;
using IdentityPrvd.WebApi.Features.Sessions.RevokeSessions;
using IdentityPrvd.WebApi.Features.Signin;
using IdentityPrvd.WebApi.Features.Signout;
using IdentityPrvd.WebApi.Features.Signup;
using IdentityPrvd.WebApi.Helpers;
using IdentityPrvd.WebApi.Middlewares;
using IdentityPrvd.WebApi.Options;
using IdentityPrvd.WebApi.Protections;
using IdentityPrvd.WebApi.ServerSideSessions;
using IdentityPrvd.WebApi.ServerSideSessions.Stores;
using IdentityPrvd.WebApi.SmsSender;
using IdentityPrvd.WebApi.UserContext;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Redis.OM;
using Redis.OM.Contracts;
using System.Text;

namespace IdentityPrvd.WebApi.Extensions;

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
        {
            var connString = identityPrvdSection["Connections:Redis"];
            return new RedisConnectionProvider(connString);
        });

        //middlewares
        services.AddTransient<CorrelationContextMiddleware>();
        services.AddTransient<ServerSideSessionMiddleware>();
        services.AddTransient<GlobalExceptionHandlerMiddleware>();
        services.AddTransient<LoggingMiddleware>();
        services.AddTransient<ETagMiddleware>();

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

        //protection
        services.AddScopedConfiguration<AppOptions>(identityPrvdSection, "App");
        services.AddScopedConfiguration<ProtectionOptions>(identityPrvdSection, "Protection");
        services.AddScoped<IProtectionService, AesProtectionService>();
        services.AddScoped<IMfaService, TotpMfaService>();
        services.AddScoped<IHasher, Sha512Hasher>();


        //others
        services.AddScoped<IUserContext, UserContext.UserContext>();
        services.AddScoped<ICurrentContext, CurrentContext.CurrentContext>();
        services.AddScoped<ISessionManager, SessionManager>();
        services.AddScoped<ISessionStore, RedisSessionStore>();
        services.AddScopedConfiguration<TokenOptions>(identityPrvdSection, "Token");
        services.AddScoped<UserHelper>();


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
}


public static class WebApplicationExtensions
{
    public static WebApplication UseIdentityPrvd(this WebApplication app)
    {
        app.UseMiddleware<CorrelationContextMiddleware>();
        app.UseMiddleware<ETagMiddleware>();
        app.UseMiddleware<LoggingMiddleware>();
        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseServerSideSessions();

        app.MapEndpoints();
        return app;
    }
}
