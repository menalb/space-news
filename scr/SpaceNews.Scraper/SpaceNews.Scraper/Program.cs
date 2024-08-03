using MongoDB.Driver;
using SmartComponents.LocalEmbeddings;
using SpaceNews.Scraper.Reader;
using Microsoft.Extensions.Configuration;
using SpaceNews.Shared.Database.Model;


var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile($"appsettings.json")
    .Build();

var connectionString = config.GetConnectionString("SpaceNews");

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
            Source = feed.Name
        });

        await newsCollection.InsertManyAsync(entities);
    }
}