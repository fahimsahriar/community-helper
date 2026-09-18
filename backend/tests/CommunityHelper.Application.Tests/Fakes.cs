using CommunityHelper.Application.Common.Interfaces;
using CommunityHelper.Domain.Entities;

namespace CommunityHelper.Application.Tests.Fakes;

public sealed class FakeJwtTokenService : IJwtTokenService
{
    private int _refreshCounter;

    public string GenerateAccessToken(User user) => $"access-{user.Id}";

    public string GenerateRefreshToken() =>
        $"refresh-{Interlocked.Increment(ref _refreshCounter)}";

    public string HashRefreshToken(string refreshToken) => $"HASH:{refreshToken}";

    public DateTime GetAccessTokenExpiry(DateTime issuedAtUtc) =>
        issuedAtUtc.AddMinutes(15);

    public DateTime GetRefreshTokenExpiry(DateTime issuedAtUtc) =>
        issuedAtUtc.AddDays(7);
}

public sealed class FakePasswordHasher : IPasswordHasher
{
    public string Hash(string password) => $"HASHED:{password}";

    public bool Verify(string password, string passwordHash) =>
        passwordHash == Hash(password);
}

public sealed class FakeCurrentUserService(string? userId, string? role) : ICurrentUserService
{
    public string? UserId => userId;
    public string? Email => userId is null ? null : "test@example.com";
    public string? Role => role;
    public bool IsAuthenticated => userId is not null;

    public bool IsInRole(string expected) =>
        string.Equals(Role, expected, StringComparison.Ordinal);
}
