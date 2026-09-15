using CommunityHelper.Application.Common.Models;

namespace CommunityHelper.Application.Common.Interfaces;

/// <summary>
/// The set of live client connections, described without reference to the
/// transport so the Application layer stays free of WebSocket types.
/// </summary>
public interface IClientConnectionRegistry
{
    bool IsConnected(string connectionId);

    /// <summary>
    /// Delivers a message to one connection.
    /// Returns <c>false</c> when the connection has gone away — an expected
    /// outcome, not an error (see docs/adr/0001, "Accepted limitation").
    /// </summary>
    Task<bool> SendAsync(string connectionId, SocketMessage message, CancellationToken ct = default);
}
