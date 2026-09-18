using System.Collections.Concurrent;
using CommunityHelper.Domain.Entities;
using CommunityHelper.Domain.Interfaces;
using CommunityHelper.Infrastructure.Persistence;

namespace CommunityHelper.Infrastructure.Repositories;

public sealed class InMemoryRefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ConcurrentDictionary<string, RefreshToken> _byId = new();
    private readonly ConcurrentDictionary<string, string> _hashToId = new();

    public Task<RefreshToken?> FindByTokenHashAsync(string tokenHash, CancellationToken ct = default) =>
        Task.FromResult(
            _hashToId.TryGetValue(tokenHash, out var id)
                ? _byId.GetValueOrDefault(id)
                : null);

    public Task InsertAsync(RefreshToken token, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(token.Id))
        {
            token.SetId(DocumentMappers.NewId());
        }

        _byId[token.Id] = token;
        _hashToId[token.TokenHash] = token.Id;
        return Task.CompletedTask;
    }

    public Task<bool> UpdateAsync(RefreshToken token, CancellationToken ct = default)
    {
        if (!_byId.ContainsKey(token.Id))
        {
            return Task.FromResult(false);
        }

        _byId[token.Id] = token;
        return Task.FromResult(true);
    }

    public Task RevokeAllForUserAsync(string userId, CancellationToken ct = default)
    {
        foreach (var token in _byId.Values.Where(t => t.UserId == userId && !t.IsRevoked))
        {
            token.Revoke();
        }

        return Task.CompletedTask;
    }
}
