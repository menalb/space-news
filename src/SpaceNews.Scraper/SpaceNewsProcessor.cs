using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SmartComponents.LocalEmbeddings;
using SpaceNews.Shared.Database.Model;
namespace SpaceNews.Scraper;

public interface ISpaceNewsProcessor
{
    Task Process();
}
public class SpaceNewsProcessor : ISpaceNewsProcessor
{
    private readonly IMongoDatabase _database;
    private readonly ILogger _logger;
    public SpaceNewsProcessor(string connectionString, ILogger logger)
    {
        _logger = logger;
        var conn = new MongoClient(connectionString);
        _database = conn.GetDatabase("SpaceNews");
    }

    public async Task Process()
    {
        var newsCollection = _database.GetNewsCollection();

        var feeds = await GetSources();

        using var embedder = new LocalEmbedder();

        foreach (var feed in feeds)
        {
            _logger.LogInformation("Name: {feedName}", feed.Name);

            Console.WriteLine($"Name: {feed.Name}");
            using (var client = new HttpClient())
            {
                var result = await new FeedReader(client).GetFeed(feed.Url);

                var entities = MapFeeds(result, feed, embedder);

                try
                {
                    await newsCollection.InsertManyAsync(entities, new InsertManyOptions { IsOrdered = false });
                }
                catch (MongoBulkWriteException<NewsEntity> ex)
                {
                    // Step 2: Handle any errors (e.g., duplicates)
                    var insertedCount = ex.Result.InsertedCount;
                    _logger.LogInformation("Inserted {insertedCount}", insertedCount);
                    _logger.LogWarning("Some documents were skipped due to duplicates.");
                }
            }
        }
    }

    private static IEnumerable<NewsEntity> MapFeeds(IEnumerable<ParsedFeed> result, SourceEntity feed, LocalEmbedder embedder)
    {
        EmbeddingF32 GenerateEmbedding(ParsedFeed feed)
        {
            var s = $"### Title: {feed.Title} ### Description: {feed.Description}";
            return embedder.Embed(s);
        }

        return result.Select(r => new NewsEntity
        {
            Title = r.Title,
            Description = r.Description,
            PublishDate = r.PublishDate.UtcDateTime,
            Links = r.Links.Select(fl => new NewsLinkEntity { Uri = fl.Uri.ToString(), Title = fl.Title }).ToArray(),
            Embeddings = GenerateEmbedding(r).Values.ToArray(),
            FeedItemId = r.Id,
            SourceId = feed.Id,
            Source = feed.Name
        });
    }

    private async Task<IList<SourceEntity>> GetSources()
    {
        var feedsCollection = _database.GetCollection<SourceEntity>("sources");
        var fs = await feedsCollection.FindAsync(f => f.IsActive);
        return await fs.ToListAsync();
    }
}