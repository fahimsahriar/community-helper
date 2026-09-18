using CommunityHelper.Domain.Entities;
using CommunityHelper.Domain.Interfaces;
using CommunityHelper.Infrastructure.Persistence;
using CommunityHelper.Infrastructure.Persistence.Documents;
using MongoDB.Driver;

namespace CommunityHelper.Infrastructure.Repositories;

public sealed class MongoVolunteerProfileRepository(MongoDbContext context) : IVolunteerProfileRepository
{
    private IMongoCollection<VolunteerProfileDocument> Collection => context.VolunteerProfiles;

    public async Task<VolunteerProfile?> FindByUserIdAsync(string userId, CancellationToken ct = default)
    {
        var doc = await Collection
            .Find(Builders<VolunteerProfileDocument>.Filter.Eq(p => p.UserId, userId))
            .FirstOrDefaultAsync(ct);
        return doc?.ToDomain();
    }

    public async Task InsertAsync(VolunteerProfile profile, CancellationToken ct = default)
    {
        var doc = profile.ToDocument();
        await Collection.InsertOneAsync(doc, cancellationToken: ct);
        profile.SetId(doc.Id);
    }

    public async Task<bool> UpdateAsync(VolunteerProfile profile, CancellationToken ct = default)
    {
        var result = await Collection.ReplaceOneAsync(
            Builders<VolunteerProfileDocument>.Filter.Eq(p => p.UserId, profile.UserId),
            profile.ToDocument(),
            cancellationToken: ct);
        return result.ModifiedCount > 0;
    }
}
