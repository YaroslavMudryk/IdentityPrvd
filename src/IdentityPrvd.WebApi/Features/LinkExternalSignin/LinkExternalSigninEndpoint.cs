using AspNet.Security.OAuth.GitHub;
using AspNet.Security.OAuth.Twitter;
using IdentityPrvd.WebApi.Api;
using IdentityPrvd.WebApi.Exceptions;
using IdentityPrvd.WebApi.Features.LinkExternalSignin.DataAccess;
using IdentityPrvd.WebApi.Features.LinkExternalSignin.Services;
using IdentityPrvd.WebApi.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityPrvd.WebApi.Features.LinkExternalSignin;

public class LinkedExternalSigninEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/identity/linked-external-signin",
            async ([FromServices] LinkedExternalSigninOrchestrator orc) =>
            {
                var linkedSignins = await orc.GetLinkedExternalSigninsAsync();
                return Results.Ok(linkedSignins.MapToResponse());
            }).WithTags("Link external signin");
    }
}

public class LinkExternalSigninEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/identity/link-external-signin",
            ([FromQuery(Name = "provider")] string provider, [FromQuery(Name = "returnUrl")] string returnUrl, LinkGenerator linkGenerator, HttpContext context) =>
            {
                if (!context.User.Identity.IsAuthenticated)
                {
                    return Results.Unauthorized();
                }

                if (string.IsNullOrEmpty(returnUrl))
                    returnUrl = linkGenerator.GetUriByName(context, "DefaultReturnUri");

                var redirectUri = $"{linkGenerator.GetPathByName(context, "LinkSigninExternalCallback")}" + $"?returnUrl={Uri.EscapeDataString(returnUrl)}&provider={provider}";
                var authProperties = new AuthenticationProperties
                {
                    RedirectUri = redirectUri,
                    Items =
                    {
                        { "CurrentUserId", context.User.FindFirst(IdentityClaims.Types.UserId)?.Value }
                    }
                };
                return Results.Challenge(authProperties, [provider]);
            }).WithTags("Link external signin");
    }
}

public class LinkExternalSigninCallbackEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/identity/link-external-signin-callback",
            async ([FromQuery(Name = "returnUrl")] string returnUrl, [FromQuery(Name = "provider")] string provider, HttpContext context, LinkExternalSigninOrchestrator orc) =>
            {
                var authenticateResult = await AuthenticateByProviderAsync(context, provider);

                await orc.LinkExternalProviderToUserAsync(authenticateResult);

                return Results.Redirect($"{returnUrl}?status=link_success");
            }).WithTags("Link external signin").WithName("LinkSigninExternalCallback");
    }

    private static async Task<AuthenticateResult> AuthenticateByProviderAsync(HttpContext context, string provider)
    {
        return provider switch
        {
            "Google" => await context.AuthenticateAsync(GoogleDefaults.AuthenticationScheme),
            "Microsoft" => await context.AuthenticateAsync(MicrosoftAccountDefaults.AuthenticationScheme),
            "GitHub" => await context.AuthenticateAsync(GitHubAuthenticationDefaults.AuthenticationScheme),
            "Facebook" => await context.AuthenticateAsync(FacebookDefaults.AuthenticationScheme),
            "Twitter" => await context.AuthenticateAsync(TwitterAuthenticationDefaults.AuthenticationScheme),
            _ => throw new BadRequestException($"Unsupported provider: {provider}"),
        };
    }
}

public class UnlinkExternalSinginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/identity/unlink-external-signin",
            async ([FromQuery(Name = "provider")] string provider, HttpContext context, UnlinkExternalSigninOrchestrator orc) =>
            {
                await orc.UnlinkExternalProviderFromUserAsync(provider);
                return Results.Ok(new { message = $"Unlinking {provider} for CurrentUser" }.MapToResponse());
            }).WithTags("Link external signin");
    }
}

public class DefaultReturnUriEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/identity/sso",
            [AllowAnonymous] async ([FromQuery(Name = "accessToken")] string accessToken, HttpContext context) =>
            {
                if (!context.User.Identity.IsAuthenticated)
                {
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        context.Request.Headers.Authorization = new Microsoft.Extensions.Primitives.StringValues($"Bearer {accessToken}");
                        var principal = JwtPrincipalFactory.CreatePrincipalFromJwt(accessToken, context.RequestServices.GetService<IConfiguration>())
                        ?? throw new UnauthorizedException("Authentication failed");

                        context.User = principal;
                    }
                }

                return Results.Ok(context.User.Claims.Select(s => new
                {
                    type = s.Type,
                    value = s.Value
                }).ToList().MapToResponse());
            }).WithName("DefaultReturnUri");
    }
}

public static class LinkExternalProviderDependencies
{
    public static void AddLinkExternalProviderDependencies(this IServiceCollection services)
    {
        services.AddScoped<LinkedExternalSigninOrchestrator>();
        services.AddScoped<LinkExternalSigninOrchestrator>();
        services.AddScoped<UnlinkExternalSigninOrchestrator>();

        services.AddScoped<LinkExternalSigninQuery>();
        services.AddScoped<UserLoginRepo>();
    }
}
