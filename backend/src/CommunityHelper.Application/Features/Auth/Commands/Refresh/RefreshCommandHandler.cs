using CommunityHelper.Application.Common.Exceptions;
using CommunityHelper.Application.Common.Interfaces;
using CommunityHelper.Domain.Entities;
using CommunityHelper.Domain.Interfaces;
using MediatR;

namespace CommunityHelper.Application.Features.Auth.Commands.Refresh;

public sealed class RefreshCommandHandler(
    IUserRepository users,
    IRefreshTokenRepository refreshTokens,
    IJwtTokenService tokens)
    : IRequestHandler<RefreshCommand, AuthResultDto>
{
    public async Task<AuthResultDto> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        var presentedHash = tokens.HashRefreshToken(request.RefreshToken);
        var stored = await refreshTokens.FindByTokenHashAsync(presentedHash, cancellationToken);
        var utcNow = DateTime.UtcNow;

        if (stored is null || stored.IsExpired(utcNow))
        {
            throw new AuthenticationException("Refresh token is invalid or expired.");
        }

        if (stored.IsRevoked)
        {
            // Reuse of a rotated-out token: the family may be compromised,
            // so revoke every token of the user before rejecting.
            await refreshTokens.RevokeAllForUserAsync(stored.UserId, cancellationToken);
            throw new AuthenticationException("Refresh token is invalid or expired.");
        }

        var user = await users.FindByIdAsync(stored.UserId, cancellationToken);
        if (user is null)
        {
            throw new AuthenticationException("Refresh token is invalid or expired.");
        }

        // Rotate: revoke the presented token and link it to its replacement.
        var issuedAt = utcNow;
        var nextRefreshToken = tokens.GenerateRefreshToken();
        var nextHash = tokens.HashRefreshToken(nextRefreshToken);

        stored.Revoke(nextHash);
        await refreshTokens.UpdateAsync(stored, cancellationToken);

        var next = RefreshToken.Create(nextHash, user.Id, tokens.GetRefreshTokenExpiry(issuedAt));
        await refreshTokens.InsertAsync(next, cancellationToken);

        return new AuthResultDto(
            tokens.GenerateAccessToken(user),
            nextRefreshToken,
            tokens.GetAccessTokenExpiry(issuedAt),
            new CurrentUserDto(user.Id, user.Email, user.Role));
    }
}
