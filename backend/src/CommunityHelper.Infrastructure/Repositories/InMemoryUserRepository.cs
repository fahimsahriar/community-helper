using System.Collections.Concurrent;
using CommunityHelper.Domain.Entities;
using CommunityHelper.Domain.Interfaces;
using CommunityHelper.Infrastructure.Persistence;

namespace CommunityHelper.Infrastructure.Repositories;

/// <summary>
/// In-memory user store. Used when no MongoDB connection is configured
/// (local dev, tests). The Mongo implementation is <c>MongoUserRepository</c>.
/// </summary>
public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<string, User> _byId = new();
    private readonly ConcurrentDictionary<string, string> _emailToId = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, string> _googleSubjectToId = new();
    private readonly object _insertLock = new();

    public Task<User?> FindByIdAsync(string id, CancellationToken ct = default) =>
        Task.FromResult(_byId.GetValueOrDefault(id));

    public Task<User?> FindByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return Task.FromResult(
            _emailToId.TryGetValue(normalized, out var id)
                ? _byId.GetValueOrDefault(id)
                : null);
    }

    public Task<User?> FindByGoogleSubjectIdAsync(string subjectId, CancellationToken ct = default) =>
        Task.FromResult(
            _googleSubjectToId.TryGetValue(subjectId, out var id)
                ? _byId.GetValueOrDefault(id)
                : null);

    public Task InsertAsync(User user, CancellationToken ct = default)
    {
        lock (_insertLock)
        {
            if (string.IsNullOrEmpty(user.Id))
            {
                user.SetId(DocumentMappers.NewId());
            }

            _byId[user.Id] = user;
            _emailToId[user.Email] = user.Id;

            if (user.GoogleSubjectId is not null)
            {
                _googleSubjectToId[user.GoogleSubjectId] = user.Id;
            }
        }

        return Task.CompletedTask;
    }

    public Task<bool> UpdateAsync(User user, CancellationToken ct = default)
    {
        if (!_byId.ContainsKey(user.Id))
        {
            return Task.FromResult(false);
        }

        _byId[user.Id] = user;

        if (user.GoogleSubjectId is not null)
        {
            _googleSubjectToId[user.GoogleSubjectId] = user.Id;
        }

        return Task.FromResult(true);
    }
}
