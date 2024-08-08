using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace SpaceNews.Shared.Database.Model;

public class NewsEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = "";
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required DateTime PublishDate { get; set; }
    public required NewsLinkEntity[] Links { get; set; }
    public required float[] Embeddings { get; set; }
    public string? FeedItemId { get; set; }
    public required string SourceId { get; set; }
    public required string Source { get; set; }
}

public class NewsLinkEntity
{
    public required string Uri { get; set; }
    public required string Title { get; set; }
}

public static class IMongoDBDatabaseExtensions
{
    public static IMongoCollection<NewsEntity> GetNewsCollection(this IMongoDatabase db) => db.GetCollection<NewsEntity>("news");
}