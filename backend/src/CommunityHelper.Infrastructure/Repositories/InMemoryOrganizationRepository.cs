using System.Collections.Concurrent;
using CommunityHelper.Domain.Entities;
using CommunityHelper.Domain.Interfaces;
using CommunityHelper.Infrastructure.Persistence;

namespace CommunityHelper.Infrastructure.Repositories;

public sealed class InMemoryOrganizationRepository : IOrganizationRepository
{
    private readonly ConcurrentDictionary<string, Organization> _byId = new();
    private readonly ConcurrentDictionary<string, string> _ownerToId = new();

    public Task<Organization?> FindByIdAsync(string id, CancellationToken ct = default) =>
        Task.FromResult(_byId.GetValueOrDefault(id));

    public Task<Organization?> FindByOwnerUserIdAsync(string ownerUserId, CancellationToken ct = default) =>
        Task.FromResult(
            _ownerToId.TryGetValue(ownerUserId, out var id)
                ? _byId.GetValueOrDefault(id)
                : null);

    public Task InsertAsync(Organization organization, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(organization.Id))
        {
            organization.SetId(DocumentMappers.NewId());
        }

        _byId[organization.Id] = organization;
        _ownerToId[organization.OwnerUserId] = organization.Id;
        return Task.CompletedTask;
    }

    public Task<bool> UpdateAsync(Organization organization, CancellationToken ct = default)
    {
        if (!_byId.ContainsKey(organization.Id))
        {
            return Task.FromResult(false);
        }

        _byId[organization.Id] = organization;
        return Task.FromResult(true);
    }
}
