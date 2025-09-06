using IdentityPrvd.Common.Api;
using IdentityPrvd.Endpoints;
using IdentityPrvd.Features.Authentication.QrSignin.Dtos;
using IdentityPrvd.Features.Authentication.QrSignin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IdentityPrvd.Features.Authentication.QrSignin;

public class QrSigninEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/auth",
            [AllowAnonymous] async (HttpContext context, IWebSocketConnectionManager manager) =>
            {
                if (!context.WebSockets.IsWebSocketRequest)
                    return Results.BadRequest(ApiResponse.Fail("Must be WebSocket request!"));

                var verificationId = context.Request.Query["verificationId"].ToString();
                var ws = await context.WebSockets.AcceptWebSocketAsync();
                manager.LinkSocketToVerification(verificationId, ws);

                var buffer = new byte[1024];
                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                while (!result.CloseStatus.HasValue)
                    result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                manager.RemoveSocket(verificationId);
                await ws.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                return Results.Ok();
            }).WithTags("QrSignin");
    }
}

public class CreateQrSigninEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/qr",
            [AllowAnonymous] async (QrRequestDto dto, IQrCodeService qrCodeService) =>
            {
                var qrCode = await qrCodeService.GenerateQrCodeAsync(dto);
                return Results.Ok(qrCode.MapToResponse());
            }).WithTags("QrSignin");
    }
}

public class GetQrSigninDetailsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/identity/qr/{verificationId}",
            async (string verificationId, IQrCodeService qrCodeService) =>
            {
                var qrCodeStatus = await qrCodeService.GetQrCodeDetailsAsync(verificationId);
                return Results.Ok(qrCodeStatus.MapToResponse());
            }).WithTags("QrSignin");
    }
}

public class ConfirmQrSigninEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/qr/confirm",
            async (QrConfirmDto confirm, IQrCodeService qrCodeService) =>
            {
                var authResult = await qrCodeService.ConfirmQrCodeAsync(confirm.VerificationId);
                return Results.Ok(authResult.MapToResponse());
            }).WithTags("QrSignin");
    }
}
