using IdentityPrvd.Common.Api;
using IdentityPrvd.Endpoints;
using IdentityPrvd.Features.Authentication.Signin.Dtos;
using IdentityPrvd.Features.Authentication.Signin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace IdentityPrvd.Features.Authentication.Signin;

public class SigninEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/signin",
            [AllowAnonymous] async (SigninRequestDto dto, SigninOrchestrator orc) =>
            {
                var result = await orc.SigninAsync(dto);
                return Results.Ok(result.MapToResponse());
            }).WithTags("Signin");
    }
}

public class SigninMfa : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/signin-mfa",
            [AllowAnonymous] async (SigninMfaRequestDto dto, SigninMfaOrchestrator orc) =>
            {
                var result = await orc.SinginMfaAsync(dto);
                return Results.Ok(result.MapToResponse());
            }).WithTags("Signin");
    }
}

public class PasswordlessSigninEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/signin-passwordless",
            [AllowAnonymous] async (PasswordlessSigninRequestDto dto, PasswordlessSigninOrchestrator orc) =>
            {
                var result = await orc.SigninAsync(dto);
                return Results.Ok(result.MapToResponse());
            }).WithTags("Signin");
    }
}

public class SigninOptionsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/identity/signin-user-options",
            [AllowAnonymous] async ([FromQuery] string login, SigninOptionsOrchestrator orc) =>
            {
                var result = await orc.GetSigninUserOptionsAsync(login);
                return Results.Ok(result.MapToResponse());
            }).WithTags("Signin");
    }
}
