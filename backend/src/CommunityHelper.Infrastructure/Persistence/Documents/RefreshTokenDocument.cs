using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CommunityHelper.Infrastructure.Persistence.Documents;

public sealed class RefreshTokenDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public string TokenHash { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public string? ReplacedByTokenHash { get; set; }
}
