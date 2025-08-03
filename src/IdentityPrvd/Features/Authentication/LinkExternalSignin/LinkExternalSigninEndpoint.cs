using IdentityPrvd.Common.Api;
using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Endpoints;
using IdentityPrvd.Features.Authentication.LinkExternalSignin.Services;
using IdentityPrvd.Services.AuthSchemes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Features.Authentication.LinkExternalSignin;

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
            async ([FromQuery(Name = "returnUrl")] string returnUrl, [FromQuery(Name = "provider")] string provider, HttpContext context, LinkExternalSigninOrchestrator orc, ExternalProviderManager providerManager) =>
            {
                var authenticateResult = await AuthenticateByProviderAsync(context, provider, providerManager);

                await orc.LinkExternalProviderToUserAsync(authenticateResult);

                return Results.Redirect($"{returnUrl}?status=link_success");
            }).WithTags("Link external signin").WithName("LinkSigninExternalCallback");
    }

    private static async Task<AuthenticateResult> AuthenticateByProviderAsync(HttpContext context, string provider, ExternalProviderManager providerManager)
    {
        return await providerManager.AuthenticateAsync(context, provider);
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
            [AllowAnonymous] ([FromQuery(Name = "accessToken")] string accessToken, HttpContext context) =>
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
            }).WithName("DefaultReturnUri").WithTags("Sso");
    }
}
