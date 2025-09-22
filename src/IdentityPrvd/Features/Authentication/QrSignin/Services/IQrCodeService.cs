using FluentValidation;
using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Common.Helpers;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Data.Transactions;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Domain.Enums;
using IdentityPrvd.Domain.ValueObjects;
using IdentityPrvd.Features.Authentication.QrSignin.Dtos;
using IdentityPrvd.Features.Shared.Dtos;
using IdentityPrvd.Mappers;
using IdentityPrvd.Options;
using IdentityPrvd.Services.Location;
using IdentityPrvd.Services.Security;
using IdentityPrvd.Services.ServerSideSessions;
using QRCoder.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Text.Json;
using Image = SixLabors.ImageSharp.Image;

namespace IdentityPrvd.Features.Authentication.QrSignin.Services;

public interface IQrCodeService
{
    Task<QrCodeDto> GenerateQrCodeAsync(QrRequestDto requestDto);
    Task<ConfirmQrDto> ConfirmQrCodeAsync(string verificationId);
    Task<ClientInfo> GetQrCodeDetailsAsync(string verificationId);
}

public class QrCodeService(
    IdentityPrvdOptions identityOptions,
    TimeProvider timeProvider,
    IUserContext userContext,
    IClientsQuery clientsQuery,
    IValidator<QrRequestDto> validator,
    ITransactionManager transactionManager,
    ISessionStore sessionStore,
    ICurrentContext currentContext,
    ILocationService locationService,
    ITokenService tokenService,
    ISessionManager sessionManager,
    IWebSocketConnectionManager manager) : IQrCodeService
{
    public async Task<ClientInfo> GetQrCodeDetailsAsync(string verificationId)
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissions(IdentityClaims.Types.Identity, IdentityClaims.Values.All);

        var qrSocket = manager.GetVerification(verificationId);
        return await Task.FromResult(qrSocket.QrRequest.Client);
    }

    public async Task<ConfirmQrDto> ConfirmQrCodeAsync(string verificationId)
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissions(IdentityClaims.Types.Identity, IdentityClaims.Values.All);

        var qrSocket = manager.GetVerification(verificationId);
        var authResult = await GetAuthAsync(currentUser, qrSocket);

        await manager.SendMessageAsync(verificationId, JsonSerializer.Serialize(authResult));

        return new ConfirmQrDto
        {
            Ok = true,
            Error = null
        };
    }

    public async Task<QrCodeDto> GenerateQrCodeAsync(QrRequestDto requestDto)
    {
        await validator.ValidateAndThrowAsync(requestDto);
        var verificationId = $"IdPrvd:{Guid.NewGuid():N}";
        var qrSocket = new QrSocket
        {
            VerificationId = verificationId,
            QrRequest = requestDto,
            WebSocket = null
        };
        manager.AddVerification(verificationId, qrSocket);

        return await Task.FromResult(new QrCodeDto
        {
            VerificationId = verificationId,
            QrBase64 = GetQrBase64(verificationId)
        });
    }

    private static string GetQrBase64(string verificationId)
    {
        using var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(verificationId, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeBytes = qrCode.GetGraphic(20);
        using var image = Image.Load<Rgba32>(qrCodeBytes);
        using var memoryStream = new MemoryStream();
        image.SaveAsPng(memoryStream);
        var imageBytes = memoryStream.ToArray();
        return Convert.ToBase64String(imageBytes);
    }

    private async Task<SigninResponseDto> GetAuthAsync(BasicAuthenticatedUser currentUser, QrSocket qrSocket)
    {
        await using var transaction = await transactionManager.BeginTransactionAsync();
        var client = await clientsQuery.GetClientByIdNullableAsync(qrSocket.QrRequest.ClientId);

        var sessionId = Ulid.NewUlid();

        var refreshToken = new IdentityRefreshToken
        {
            SessionId = sessionId,
            Value = Generator.GetRefreshToken(),
            ExpiredAt = timeProvider.GetUtcNow().UtcDateTime.AddDays(identityOptions.Token.RefreshLifeTimeInDays)
        };

        var userId = currentUser.UserId.GetIdAsUlid();
        var location = await locationService.GetIpInfoAsync(currentContext.IpAddress);

        var newSession = new IdentitySession
        {
            Id = sessionId,
            UserId = userId,
            Client = qrSocket.QrRequest.Client,
            Data = qrSocket.QrRequest.Data,
            App = client.MapToAppInfo(qrSocket.QrRequest.AppVersion),
            Location = location,
            Language = qrSocket.QrRequest.Language,
            AuthorizedBy = currentUser.SessionId.GetIdAsUlid(),
            Status = SessionStatus.Active,
            Type = SessionType.Qr,
            ExpireAt = timeProvider.GetUtcNow().UtcDateTime.AddDays(identityOptions.Token.SessionLifeTimeInDays),
            ViaMfa = false,
            Tokens = [refreshToken]
        };

        await sessionStore.AddAsync(newSession);

        var jwtToken = await tokenService.GetUserTokenAsync(userId, sessionId.GetIdAsString(), client.Name);

        var userPermissions = await tokenService.GetUserPermissionsAsync(userId, qrSocket.QrRequest.ClientId);

        await sessionManager.AddNewSessionAsync(new SessionInfo
        {
            CreatedAt = newSession.CreatedAt,
            LastAccessedAt = null,
            Permissions = userPermissions,
            SessionExpire = newSession.ExpireAt,
            SessionId = newSession.Id.ToString(),
            UserId = newSession.UserId.ToString(),
        });

        await transaction.CommitAsync();

        return new SigninResponseDto
        {
            RequiredMfa = false,
            VerifyId = null,
            AccessToken = jwtToken.Token,
            RefreshToken = refreshToken.Value,
            ExpireIn = identityOptions.Token.LifeTimeInMinutes * 60,
        };
    }
}
