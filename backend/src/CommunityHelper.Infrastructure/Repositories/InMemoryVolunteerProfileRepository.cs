using System.Collections.Concurrent;
using CommunityHelper.Domain.Entities;
using CommunityHelper.Domain.Interfaces;
using CommunityHelper.Infrastructure.Persistence;

namespace CommunityHelper.Infrastructure.Repositories;

public sealed class InMemoryVolunteerProfileRepository : IVolunteerProfileRepository
{
    private readonly ConcurrentDictionary<string, VolunteerProfile> _byUserId = new();

    public Task<VolunteerProfile?> FindByUserIdAsync(string userId, CancellationToken ct = default) =>
        Task.FromResult(_byUserId.GetValueOrDefault(userId));

    public Task InsertAsync(VolunteerProfile profile, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(profile.Id))
        {
            profile.SetId(DocumentMappers.NewId());
        }

        _byUserId[profile.UserId] = profile;
        return Task.CompletedTask;
    }

    public Task<bool> UpdateAsync(VolunteerProfile profile, CancellationToken ct = default)
    {
        if (!_byUserId.ContainsKey(profile.UserId))
        {
            return Task.FromResult(false);
        }

        _byUserId[profile.UserId] = profile;
        return Task.FromResult(true);
    }
}
