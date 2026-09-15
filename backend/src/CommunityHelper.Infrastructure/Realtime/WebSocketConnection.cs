using System.Net.WebSockets;

namespace CommunityHelper.Infrastructure.Realtime;

/// <summary>
/// One live client socket. Sends are serialized because
/// <see cref="WebSocket.SendAsync(ReadOnlyMemory{byte}, WebSocketMessageType, bool, CancellationToken)"/>
/// is not safe for concurrent callers, and the worker can push while the
/// connection handler is still writing the handshake.
/// </summary>
public sealed class WebSocketConnection(string id, WebSocket socket) : IDisposable
{
    private readonly SemaphoreSlim _sendLock = new(1, 1);

    public string Id { get; } = id;
    public WebSocket Socket { get; } = socket;

    public bool IsOpen => Socket.State == WebSocketState.Open;

    public async Task SendAsync(ReadOnlyMemory<byte> payload, CancellationToken ct = default)
    {
        await _sendLock.WaitAsync(ct);
        try
        {
            if (!IsOpen)
            {
                return;
            }

            await Socket.SendAsync(payload, WebSocketMessageType.Text, endOfMessage: true, ct);
        }
        finally
        {
            _sendLock.Release();
        }
    }

    public void Dispose() => _sendLock.Dispose();
}
