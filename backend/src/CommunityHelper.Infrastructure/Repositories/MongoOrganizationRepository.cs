using CommunityHelper.Domain.Entities;
using CommunityHelper.Domain.Interfaces;
using CommunityHelper.Infrastructure.Persistence;
using CommunityHelper.Infrastructure.Persistence.Documents;
using MongoDB.Driver;

namespace CommunityHelper.Infrastructure.Repositories;

public sealed class MongoOrganizationRepository(MongoDbContext context) : IOrganizationRepository
{
    private IMongoCollection<OrganizationDocument> Collection => context.Organizations;

    public async Task<Organization?> FindByIdAsync(string id, CancellationToken ct = default)
    {
        if (!DocumentMappers.IsObjectId(id))
        {
            return null;
        }

        var doc = await Collection
            .Find(Builders<OrganizationDocument>.Filter.Eq(o => o.Id, id))
            .FirstOrDefaultAsync(ct);
        return doc?.ToDomain();
    }

    public async Task<Organization?> FindByOwnerUserIdAsync(string ownerUserId, CancellationToken ct = default)
    {
        var doc = await Collection
            .Find(Builders<OrganizationDocument>.Filter.Eq(o => o.OwnerUserId, ownerUserId))
            .FirstOrDefaultAsync(ct);
        return doc?.ToDomain();
    }

    public async Task InsertAsync(Organization organization, CancellationToken ct = default)
    {
        var doc = organization.ToDocument();
        await Collection.InsertOneAsync(doc, cancellationToken: ct);
        organization.SetId(doc.Id);
    }

    public async Task<bool> UpdateAsync(Organization organization, CancellationToken ct = default)
    {
        var result = await Collection.ReplaceOneAsync(
            Builders<OrganizationDocument>.Filter.Eq(o => o.Id, organization.Id),
            organization.ToDocument(),
            cancellationToken: ct);
        return result.ModifiedCount > 0;
    }
}
