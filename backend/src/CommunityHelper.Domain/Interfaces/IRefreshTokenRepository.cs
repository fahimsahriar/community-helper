using CommunityHelper.Domain.Entities;

namespace CommunityHelper.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> FindByTokenHashAsync(string tokenHash, CancellationToken ct = default);
    Task InsertAsync(RefreshToken token, CancellationToken ct = default);
    Task<bool> UpdateAsync(RefreshToken token, CancellationToken ct = default);

    /// <summary>
    /// Revokes every active token of the user (reuse detection + logout-all).
    /// </summary>
    Task RevokeAllForUserAsync(string userId, CancellationToken ct = default);
}
