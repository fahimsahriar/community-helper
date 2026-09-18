using CommunityHelper.Application.Common.Interfaces;
using CommunityHelper.Domain.Interfaces;
using MediatR;

namespace CommunityHelper.Application.Features.Auth.Commands.Logout;

/// <summary>
/// Revokes the presented refresh token. Unknown tokens are a no-op success
/// so logout cannot be used as a token oracle.
/// </summary>
public sealed class LogoutCommandHandler(
    IRefreshTokenRepository refreshTokens,
    IJwtTokenService tokens)
    : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var hash = tokens.HashRefreshToken(request.RefreshToken);
        var stored = await refreshTokens.FindByTokenHashAsync(hash, cancellationToken);

        if (stored is null || stored.IsRevoked)
        {
            return;
        }

        stored.Revoke();
        await refreshTokens.UpdateAsync(stored, cancellationToken);
    }
}
