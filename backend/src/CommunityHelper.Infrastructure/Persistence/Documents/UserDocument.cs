using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CommunityHelper.Infrastructure.Persistence.Documents;

public sealed class UserDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public string? GoogleSubjectId { get; set; }
}
