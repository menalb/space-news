using Amazon.Lambda.Core;
using MongoDB.Driver;
using SmartComponents.LocalEmbeddings;
using SpaceNews.Shared.Database.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SpaceNews.Updater;

public class Function
{
    
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public async Task FunctionHandler(ILambdaContext context)
    {
        // var connectionString = config.GetConnectionString("SpaceNews");
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

        var conn = new MongoClient(connectionString);
        var database = conn.GetDatabase("SpaceNews");
        var newsCollection = database.GetNewsCollection();

        var feedsCollection = database.GetCollection<SourceEntity>("sources");

        var fs = await feedsCollection.FindAsync(f => true);
        var feeds = await fs.ToListAsync();


        using var embedder = new LocalEmbedder();

        EmbeddingF32 GenerateEmbedding(ParsedFeed feed)
        {
            var s = $"### Title: {feed.Title} ### Description: {feed.Description}";
            return embedder.Embed(s);
        }

        foreach (var feed in feeds)
        {
            Console.WriteLine($"Name: {feed.Name}");
            using (var client = new HttpClient())
            {
                var result = await new FeedReader(client).GetFeed(feed.Url);

                var entities = result.Select(r => new NewsEntity
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

                try
                {
                    await newsCollection.InsertManyAsync(entities, new InsertManyOptions { IsOrdered = false });
                }
                catch (MongoBulkWriteException<NewsEntity> ex)
                {
                    // Step 2: Handle any errors (e.g., duplicates)
                    var insertedCount = ex.Result.InsertedCount;
                    Console.WriteLine($"Documents inserted: {insertedCount}");
                    Console.WriteLine("Some documents were skipped due to duplicates.");
                }
            }
        }
    }
}
