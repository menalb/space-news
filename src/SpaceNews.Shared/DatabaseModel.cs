using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace SpaceNews.Shared.Database.Model;

public class NewsEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required DateTime PublishDate { get; set; }
    public required NewsLinkEntity[] Links { get; set; }
    public required float[] Embeddings { get; set; }
    public string? FeedItemId { get; set; }
    [BsonRepresentation(BsonType.ObjectId)]
    public required string SourceId { get; set; }
    public required string Source { get; set; }
}

public class NewsLinkEntity
{
    public required string Uri { get; set; }
    public required string Title { get; set; }
}

public class SourceEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = "";
    public required string Name { get; set; }
    public required string Url { get; set; }
    public bool ExcludeFromSummary { get; set; } = false;
}

public class SummaryEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = "";
    public required string Summary { get; set; }
    public required DateTime DateTime { get; set; }
    public SummaryPart[]? SummaryParts { get; set; }
}

public class SummaryPart
{
    public required string[] NewsId { get; set; }
    public required string Summary { get; set; }
}

public static class IMongoDBDatabaseExtensions
{
    public static IMongoCollection<NewsEntity> GetNewsCollection(this IMongoDatabase db) => db.GetCollection<NewsEntity>("news");
    public static IMongoCollection<SourceEntity> GetSourcesCollection(this IMongoDatabase db) => db.GetCollection<SourceEntity>("sources");
    public static IMongoCollection<SummaryEntity> GetSummariesCollection(this IMongoDatabase db) => db.GetCollection<SummaryEntity>("summaries");
}