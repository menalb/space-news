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
    private readonly string _youTubeAPIKey;
    private readonly ILogger _logger;
    public SpaceNewsProcessor(string connectionString,string youTubeAPIKey, ILogger logger)
    {
        _logger = logger;
        _youTubeAPIKey = youTubeAPIKey;
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
                var entities = feed.Type == "video"
                    ? await new VideoReader(client, _youTubeAPIKey).GetVideoData(DateTime.UtcNow.AddDays(-10), feed, embedder)
                    : await new FeedReader(client).GetFeed(feed, embedder);

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

    private async Task<IList<SourceEntity>> GetSources()
    {
        var feedsCollection = _database.GetCollection<SourceEntity>("sources");
        var fs = await feedsCollection.FindAsync(f => f.IsActive || f.Type == "video");
        return await fs.ToListAsync();
    }
}