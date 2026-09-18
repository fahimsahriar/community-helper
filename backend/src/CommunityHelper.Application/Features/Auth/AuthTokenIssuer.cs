using CommunityHelper.Application.Common.Interfaces;
using CommunityHelper.Domain.Entities;
using CommunityHelper.Domain.Interfaces;

namespace CommunityHelper.Application.Features.Auth;

/// <summary>
/// Issues and persists the access + rotating-refresh pair for a user.
/// Shared by register, login, refresh, and Google handlers so rotation
/// semantics live in exactly one place.
/// </summary>
public sealed class AuthTokenIssuer(
    IJwtTokenService tokens,
    IRefreshTokenRepository refreshTokens)
{
    public async Task<AuthResultDto> IssueAsync(User user, CancellationToken cancellationToken)
    {
        var issuedAt = DateTime.UtcNow;
        var accessToken = tokens.GenerateAccessToken(user);
        var refreshToken = tokens.GenerateRefreshToken();

        var stored = RefreshToken.Create(
            tokens.HashRefreshToken(refreshToken),
            user.Id,
            tokens.GetRefreshTokenExpiry(issuedAt));

        await refreshTokens.InsertAsync(stored, cancellationToken);

        return new AuthResultDto(
            accessToken,
            refreshToken,
            tokens.GetAccessTokenExpiry(issuedAt),
            new CurrentUserDto(user.Id, user.Email, user.Role));
    }
}
