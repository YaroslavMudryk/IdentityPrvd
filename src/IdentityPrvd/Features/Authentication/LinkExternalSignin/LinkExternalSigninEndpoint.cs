using IdentityPrvd.Common.Api;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Endpoints;
using IdentityPrvd.Features.Authentication.LinkExternalSignin.Services;
using IdentityPrvd.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
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

// LinkExternalSigninEndpoint and LinkExternalSigninCallbackEndpoint are now handled by 
// ExternalSigninEndpoint with purpose=link parameter

public class UnlinkExternalSinginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/identity/unlink-external-signin",
            async ([FromQuery(Name = "provider")] string provider,
            HttpContext context,
            UnlinkExternalSigninOrchestrator orc) =>
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
            [AllowAnonymous] ([FromQuery(Name = "accessToken")] string accessToken,
            HttpContext context) =>
            {
                if (!context.User.Identity.IsAuthenticated)
                {
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        var principal = JwtPrincipalFactory.CreatePrincipalFromJwt(accessToken, context.RequestServices.GetService<IdentityPrvdOptions>())
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
