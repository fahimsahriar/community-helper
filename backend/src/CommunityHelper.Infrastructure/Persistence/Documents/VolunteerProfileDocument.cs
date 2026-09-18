using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CommunityHelper.Infrastructure.Persistence.Documents;

public sealed class VolunteerProfileDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = [];
    public string Availability { get; set; } = string.Empty;
    public List<string> Causes { get; set; } = [];
    public string Location { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
}
