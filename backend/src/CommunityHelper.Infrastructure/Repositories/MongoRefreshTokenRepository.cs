using CommunityHelper.Domain.Entities;
using CommunityHelper.Domain.Interfaces;
using CommunityHelper.Infrastructure.Persistence;
using CommunityHelper.Infrastructure.Persistence.Documents;
using MongoDB.Driver;

namespace CommunityHelper.Infrastructure.Repositories;

public sealed class MongoRefreshTokenRepository(MongoDbContext context) : IRefreshTokenRepository
{
    private IMongoCollection<RefreshTokenDocument> Collection => context.RefreshTokens;

    public async Task<RefreshToken?> FindByTokenHashAsync(string tokenHash, CancellationToken ct = default)
    {
        var doc = await Collection
            .Find(Builders<RefreshTokenDocument>.Filter.Eq(t => t.TokenHash, tokenHash))
            .FirstOrDefaultAsync(ct);
        return doc?.ToDomain();
    }

    public async Task InsertAsync(RefreshToken token, CancellationToken ct = default)
    {
        var doc = token.ToDocument();
        await Collection.InsertOneAsync(doc, cancellationToken: ct);
        token.SetId(doc.Id);
    }

    public async Task<bool> UpdateAsync(RefreshToken token, CancellationToken ct = default)
    {
        var result = await Collection.ReplaceOneAsync(
            Builders<RefreshTokenDocument>.Filter.Eq(t => t.Id, token.Id),
            token.ToDocument(),
            cancellationToken: ct);
        return result.ModifiedCount > 0;
    }

    public async Task RevokeAllForUserAsync(string userId, CancellationToken ct = default)
    {
        var utcNow = DateTime.UtcNow;
        await Collection.UpdateManyAsync(
            Builders<RefreshTokenDocument>.Filter.And(
                Builders<RefreshTokenDocument>.Filter.Eq(t => t.UserId, userId),
                Builders<RefreshTokenDocument>.Filter.Eq(t => t.RevokedAtUtc, null)),
            Builders<RefreshTokenDocument>.Update.Set(t => t.RevokedAtUtc, utcNow),
            cancellationToken: ct);
    }
}
