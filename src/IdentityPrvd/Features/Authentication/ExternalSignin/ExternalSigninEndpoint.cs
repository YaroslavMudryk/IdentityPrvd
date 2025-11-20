using FluentValidation;
using IdentityPrvd.Common.Helpers;
using IdentityPrvd.Endpoints;
using IdentityPrvd.Features.Authentication.ExternalSignin.Dtos;
using IdentityPrvd.Features.Authentication.ExternalSignin.Services;
using IdentityPrvd.Services.AuthSchemes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace IdentityPrvd.Features.Authentication.ExternalSignin;

public class ExternalSigninEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/identity/signin-external",
            async (
                [AsParameters] ExternalSigninDto dto,
                IValidator<ExternalSigninDto> validator,
                LinkGenerator linkGenerator,
                HttpContext context) =>
            {
                await ValidationHelper.ValidateAndThrowAsync(validator, dto);

                // Validate purpose
                if (string.IsNullOrEmpty(dto.Purpose))
                    dto.Purpose = "login";
                
                if (dto.Purpose != "login" && dto.Purpose != "link")
                    return Results.BadRequest("Purpose must be 'login' or 'link'");

                // For link purpose, require authentication
                if (dto.Purpose == "link" && !context.User.Identity.IsAuthenticated)
                    return Results.Unauthorized();

                if (string.IsNullOrEmpty(dto.ReturnUrl))
                    dto.ReturnUrl = linkGenerator.GetUriByName(context, "DefaultReturnUri");

                var callbackName = dto.Purpose == "link" ? "LinkSigninExternalCallback" : "SigninExternalCallback";
                var authProperties = new AuthenticationProperties
                {
                    RedirectUri = $"{linkGenerator.GetPathByName(context, callbackName)}" +
                        $"?returnUrl={Uri.EscapeDataString(dto.ReturnUrl)}&provider={dto.Provider}&purpose={dto.Purpose}"
                };
                authProperties.Items.SetupItemsFromDto(dto);
                
                // For link purpose, store current user ID
                if (dto.Purpose == "link" && context.User.Identity.IsAuthenticated)
                {
                    authProperties.Items.Add("CurrentUserId", context.User.FindFirst(IdentityPrvd.Common.Constants.IdentityClaims.Types.UserId)?.Value);
                }

                return Results.Challenge(authProperties, [dto.Provider]);
            }).WithTags("External signin");
    }
}

public class ExternalSigninCallbackEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/identity/signin-external-callback",
            [AllowAnonymous] async (
                [FromQuery(Name = "returnUrl")] string returnUrl,
                [FromQuery(Name = "provider")] string provider,
                [FromQuery(Name = "purpose")] string purpose,
                HttpContext context, 
                ExternalSigninOrchestrator signinOrc,
                LinkExternalSignin.Services.LinkExternalSigninOrchestrator linkOrc,
                ExternalProviderManager providerManager) =>
            {
                var authResult = await providerManager.AuthenticateAsync(context, provider);
                var purposeValue = purpose ?? "login";

                if (purposeValue == "link")
                {
                    await linkOrc.LinkExternalProviderToUserAsync(authResult);
                    return Results.Redirect($"{returnUrl}?status=link_success");
                }
                else
                {
                    var responseDto = await signinOrc.SigninExternalProviderAsync(authResult);
                    return Results.Redirect($"{returnUrl}?accessToken={responseDto.AccessToken}&refreshToken={responseDto.RefreshToken}&expireIn={responseDto.ExpireIn}");
                }
            }).WithTags("External signin").WithName("SigninExternalCallback");
        
        // Keep old callback name for backward compatibility
        app.MapGet("/api/identity/link-external-signin-callback",
            [AllowAnonymous] async (
                [FromQuery(Name = "returnUrl")] string returnUrl, 
                [FromQuery(Name = "provider")] string provider,
                HttpContext context,
                LinkExternalSignin.Services.LinkExternalSigninOrchestrator linkOrc,
                ExternalProviderManager providerManager) =>
            {
                var authResult = await providerManager.AuthenticateAsync(context, provider);
                await linkOrc.LinkExternalProviderToUserAsync(authResult);
                return Results.Redirect($"{returnUrl}?status=link_success");
            }).WithTags("External signin").WithName("LinkSigninExternalCallback");
    }
}
