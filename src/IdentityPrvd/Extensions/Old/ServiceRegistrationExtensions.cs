using IdentityPrvd.Common.Helpers;
using IdentityPrvd.Contexts;
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
using IdentityPrvd.Infrastructure.Middleware;
using IdentityPrvd.Services.AuthSchemes;
using IdentityPrvd.Services.Security;
using IdentityPrvd.Services.ServerSideSessions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IdentityPrvd.Extensions.Old;

public static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddFeatures(this IServiceCollection services)
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

    public static IServiceCollection AddMiddlewares(this IServiceCollection services)
    {
        services.AddTransient<CorrelationContextMiddleware>();
        services.AddTransient<ServerSideSessionMiddleware>();
        services.AddTransient<GlobalExceptionHandlerMiddleware>();        
        return services;
    }

    public static IServiceCollection AddContexts(this IServiceCollection services)
    {
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<ICurrentContext, CurrentContext>();        
        return services;
    }

    public static IServiceCollection AddProtectionServices(this IServiceCollection services)
    {
        services.AddScoped<IProtectionService, AesProtectionService>();
        services.AddScoped<IMfaService, TotpMfaService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IHasher, Sha512Hasher>();        
        return services;
    }

    public static IServiceCollection AddOthers(this IServiceCollection services)
    {
        services.AddScoped<UserHelper>();
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IAuthSchemes, DefaultAuthSchemes>();        
        return services;
    }
} 