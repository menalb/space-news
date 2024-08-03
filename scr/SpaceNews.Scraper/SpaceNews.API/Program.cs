using MongoDB.Driver;
using SmartComponents.LocalEmbeddings;
using SpaceNews.Shared.Database.Model;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("SpaceNews");
var conn = new MongoClient(connectionString);
var database = conn.GetDatabase("SpaceNews");
builder.Services.AddSingleton(database);

var app = builder.Build();

app.MapGet("/news", async (string text, IMongoDatabase db) =>
{
    using var embedder = new LocalEmbedder();
    var target = embedder.Embed(text);
    var options = new VectorSearchOptions<NewsEntity>()
    {
        IndexName = "news_vector_index",
        NumberOfCandidates = 150
    };

    var agg = database
    .GetNewsCollection()
    .Aggregate()
    .VectorSearch(m => m.Embeddings, target.Values, 5, options);

    var result = await agg.Project(Builders<NewsEntity>.Projection.Expression(f => new Entry(
        f.Title,
        f.Description,
        f.PublishDate,
        f.Links,
        f.Source
    ))).ToListAsync();

    return result;
});

app.Run();

public record Entry(string Title, string Description, DateTime PublishDate, NewsLinkEntity[] Links, string Source);