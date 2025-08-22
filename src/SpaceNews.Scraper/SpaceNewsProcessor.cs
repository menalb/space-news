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
    private readonly IVideoReader _videoReader;
    private readonly IFeedReader _feedReader;
    private readonly ILogger _logger;
    // public SpaceNewsProcessor(string connectionString, string youTubeAPIKey, ILogger logger)
    public SpaceNewsProcessor(string connectionString, IFeedReader feedReader, IVideoReader videoReader, ILogger logger)
    {
        _logger = logger;
        var conn = new MongoClient(connectionString);
        _database = conn.GetDatabase("SpaceNews");
        _feedReader = feedReader;
        _videoReader = videoReader;
    }

    public async Task Process()
    {
        var newsCollection = _database.GetNewsCollection();

        var feeds = await GetSources();

        using var embedder = new LocalEmbedder();
        EmbeddingF32 GenerateEmbedding(string title, string description)
        {
            var s = $"### Title: {title} ### Description: {description}";
            return embedder.Embed(s);
        }

        foreach (var feed in feeds)
        {
            _logger.LogInformation("Name: {feedName}", feed.Name);
            var entities = feed.Type == "video" && feed.ChannelId is not null
                ? await _videoReader
                .GetVideoData(DateTime.UtcNow.AddDays(-10), feed.ChannelId)
                : await _feedReader.GetFeed(feed.Url);            

            var embeddedEntries = entities.Select(e => new NewsEntity
            {
                Description = e.Description,
                FeedItemId = e.ItemId,
                Links = e.Links,
                PublishDate = e.PublishDate,
                SourceId = feed.Id,
                Source = feed.Name,
                Title = e.Title,
                Embeddings = GenerateEmbedding(e.Title, e.Description).Values.ToArray()
            });

            _logger.LogInformation("Entries: {entries}", embeddedEntries.Count());

            try
            {
                await newsCollection.InsertManyAsync(embeddedEntries, new InsertManyOptions { IsOrdered = false });
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

    private async Task<IList<SourceEntity>> GetSources()
    {
        var feedsCollection = _database.GetCollection<SourceEntity>("sources");
        var fs = await feedsCollection.FindAsync(f => f.IsActive || f.Type == "video");
        return await fs.ToListAsync();
    }
}