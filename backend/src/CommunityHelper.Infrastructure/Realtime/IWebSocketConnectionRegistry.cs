using System.Net.WebSockets;
using CommunityHelper.Application.Common.Interfaces;

namespace CommunityHelper.Infrastructure.Realtime;

/// <summary>
/// Adds transport-level registration to the Application-facing registry port.
/// Only the API layer's socket endpoint needs these members.
/// </summary>
public interface IWebSocketConnectionRegistry : IClientConnectionRegistry
{
    WebSocketConnection Add(WebSocket socket);

    void Remove(string connectionId);

    int Count { get; }
}
