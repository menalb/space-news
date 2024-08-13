using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace SpaceNews.Updater;

public class SourceEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = "";
    public required string Name { get; set; }
    public required string Url { get; set; }
}
