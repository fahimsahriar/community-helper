using CommunityHelper.Domain.Entities;

namespace CommunityHelper.Application.Common.Interfaces;

/// <summary>
/// Issues JWT access tokens and opaque rotating refresh tokens.
/// </summary>
public interface IJwtTokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    string HashRefreshToken(string refreshToken);
    DateTime GetAccessTokenExpiry(DateTime issuedAtUtc);
    DateTime GetRefreshTokenExpiry(DateTime issuedAtUtc);
}
