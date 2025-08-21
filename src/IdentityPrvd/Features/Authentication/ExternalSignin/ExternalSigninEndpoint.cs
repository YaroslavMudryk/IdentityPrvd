using FluentValidation;
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
            [AllowAnonymous] async (
                [AsParameters] ExternalSigninDto dto,
                IValidator<ExternalSigninDto> validator,
                LinkGenerator linkGenerator,
                HttpContext context) =>
            {
                await validator.ValidateAndThrowAsync(dto);

                if (string.IsNullOrEmpty(dto.ReturnUrl))
                    dto.ReturnUrl = linkGenerator.GetUriByName(context, "DefaultReturnUri");

                var authProperties = new AuthenticationProperties
                {
                    RedirectUri = $"{linkGenerator.GetPathByName(context, "SigninExternalCallback")}" +
                        $"?returnUrl={Uri.EscapeDataString(dto.ReturnUrl)}&provider={dto.Provider}"
                };
                authProperties.Items.SetupItemsFromDto(dto);
                return Results.Challenge(authProperties, [dto.Provider]);
            }).WithTags("External signin");
    }
}

public class ExternalSigninCallbackEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/identity/signin-external-callback",
            [AllowAnonymous] async ([FromQuery(Name = "returnUrl")] string returnUrl, [FromQuery(Name = "provider")] string provider, HttpContext context, ExternalSigninOrchestrator orc, ExternalProviderManager providerManager) =>
            {
                var responseDto = await orc.SigninExternalProviderAsync(await providerManager.AuthenticateAsync(context, provider));
                return Results.Redirect($"{returnUrl}?accessToken={responseDto.AccessToken}&refreshToken={responseDto.RefreshToken}&expiredIn={responseDto.ExpiredIn}");
            }).WithTags("External signin", "IdentityPrvd").WithName("SigninExternalCallback");
    }
}
