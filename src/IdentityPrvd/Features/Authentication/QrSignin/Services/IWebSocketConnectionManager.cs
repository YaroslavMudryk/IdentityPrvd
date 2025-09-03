using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Features.Authentication.QrSignin.Dtos;
using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace IdentityPrvd.Features.Authentication.QrSignin.Services;

public interface IWebSocketConnectionManager
{
    void AddVerification(string verificationId, QrSocket qrSocket);
    void LinkSocketToVerification(string verificationId, WebSocket webSocket);
    QrSocket GetVerification(string verificationId);
    Task SendMessageAsync(string verificationId, string message);
    void RemoveSocket(string verificationId);
}

public class WebSocketConnectionManager : IWebSocketConnectionManager
{
    private readonly ConcurrentDictionary<string, QrSocket> _sockets = new();

    public void AddVerification(string verificationId, QrSocket qrSocket)
    {
        if (!_sockets.TryAdd(verificationId, qrSocket))
            throw new InvalidOperationException("Verification ID already exists.");

        _sockets[verificationId] = qrSocket;
    }

    public void LinkSocketToVerification(string verificationId, WebSocket webSocket)
    {
        if (!_sockets.TryGetValue(verificationId, out var qrSocket))
            throw new HttpResponseException((int)HttpStatusCode.BadRequest, "Verification ID not exists");

        qrSocket.WebSocket = webSocket;
    }

    public async Task SendMessageAsync(string verificationId, string message)
    {
        if (_sockets.TryGetValue(verificationId, out var qrSocket) &&
            qrSocket.WebSocket.State == WebSocketState.Open)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await qrSocket.WebSocket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    public void RemoveSocket(string verificationId)
    {
        _sockets.TryRemove(verificationId, out _);
    }

    public QrSocket GetVerification(string verificationId)
    {
        if (_sockets.TryGetValue(verificationId, out var qrSocket))
            return qrSocket;

        throw new HttpResponseException((int)HttpStatusCode.BadRequest, "Verification ID not exists");
    }
}

public class QrSocket
{
    public string VerificationId { get; set; }
    public WebSocket WebSocket { get; set; }
    public QrRequestDto QrRequest { get; set; }
}
