using CommunityHelper.Domain.Entities;

namespace CommunityHelper.Domain.Tests.Entities;

public class RefreshTokenTests
{
    private static RefreshToken Create(DateTime? expiresAtUtc = null) =>
        RefreshToken.Create("hash-1", "user-1", expiresAtUtc ?? DateTime.UtcNow.AddDays(7));

    [Fact]
    public void Create_RejectsPastExpiry()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            RefreshToken.Create("hash-1", "user-1", DateTime.UtcNow.AddMinutes(-1)));
        Assert.Equal("expiresAtUtc", ex.ParamName);
    }

    [Fact]
    public void NewToken_IsActive()
    {
        var token = Create();

        Assert.True(token.IsActive(DateTime.UtcNow));
        Assert.False(token.IsRevoked);
        Assert.False(token.IsExpired(DateTime.UtcNow));
    }

    [Fact]
    public void Revoke_MarksRevokedAndLinksReplacement()
    {
        var token = Create();

        token.Revoke("hash-2");

        Assert.True(token.IsRevoked);
        Assert.False(token.IsActive(DateTime.UtcNow));
        Assert.Equal("hash-2", token.ReplacedByTokenHash);
        Assert.NotNull(token.RevokedAtUtc);
    }

    [Fact]
    public void Rehydrate_RestoresExpiredStoredToken()
    {
        // Stored tokens may already be expired; rehydration must not throw.
        var token = RefreshToken.Rehydrate(
            "id-1", "hash-1", "user-1",
            DateTime.UtcNow.AddDays(-8), DateTime.UtcNow.AddDays(-1),
            revokedAtUtc: null, replacedByTokenHash: null);

        Assert.True(token.IsExpired(DateTime.UtcNow));
        Assert.False(token.IsActive(DateTime.UtcNow));
    }
}
