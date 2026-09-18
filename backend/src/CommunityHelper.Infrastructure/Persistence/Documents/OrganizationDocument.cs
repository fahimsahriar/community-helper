using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CommunityHelper.Infrastructure.Persistence.Documents;

public sealed class OrganizationDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public string OwnerUserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public List<string> CauseTags { get; set; } = [];
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? VerifiedAtUtc { get; set; }
}
