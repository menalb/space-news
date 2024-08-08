using MongoDB.Driver;
using MongoDB.Driver.Search;
using SmartComponents.LocalEmbeddings;
using SpaceNews.Shared.Database.Model;

var builder = WebApplication.CreateBuilder(args);

var SpaceNewsApiOriginPolicy = "_spaceNewsApiOriginPolicy";

var allowedOrigin = builder.Environment.IsDevelopment() ?
    builder.Configuration.GetValue<string>("AllowedOrigin") :
    builder.Configuration.GetValue<string>("ALLOWED_ORIGIN");

builder.Services.AddCors(options =>
{
    if(allowedOrigin is not null)
    {
        options.AddPolicy(name: SpaceNewsApiOriginPolicy,
            policy =>
            {
                policy.WithOrigins(allowedOrigin);
            });
    }
 });

builder.Services.AddControllers();

// Add AWS Lambda support. When application is run in Lambda Kestrel is swapped out as the web server with Amazon.Lambda.AspNetCoreServer. This
// package will act as the webserver translating request and responses between the Lambda event source and ASP.NET Core.
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

var connectionString = builder.Environment.IsDevelopment() ?
     builder.Configuration.GetConnectionString("SpaceNews") :
      builder.Configuration.GetValue<string>("DB_CONNECTION_STRING");

var conn = new MongoClient(connectionString);
var database = conn.GetDatabase("SpaceNews");
builder.Services.AddSingleton(database);

var app = builder.Build();

app.UseHttpsRedirection();

app.UseCors(SpaceNewsApiOriginPolicy);
app.UseAuthorization();

app.MapGet("/news/semantic", async (string search, IMongoDatabase db) =>
{
    using var embedder = new LocalEmbedder();
    var target = embedder.Embed(search);
    var options = new VectorSearchOptions<NewsEntity>()
    {
        IndexName = "news_vector_index",
        NumberOfCandidates = 150
    };

    var agg = db
    .GetNewsCollection()
    .Aggregate()
    .VectorSearch(m => m.Embeddings, target.Values, 10, options);

    var result = await ProjectToEntry(agg).ToListAsync();

    return result;
});

app.MapGet("/news/text", async (string search, IMongoDatabase db) =>
{
    var fuzzyOptions = new SearchFuzzyOptions()
    {
        MaxEdits = 1,
        PrefixLength = 1,
        MaxExpansions = 256
    };

    var agg = db
    .GetNewsCollection()
    .Aggregate()
    .Search(
        Builders<NewsEntity>.Search.Text(x => x.Title, search, fuzzyOptions),
        indexName: "news_text_index")
    .SortByDescending<NewsEntity>(x => x.PublishDate);

    var result = await ProjectToEntry(agg).ToListAsync();

    return result;
});

app.MapGet("/news", async (IMongoDatabase db) =>
{
    var agg = db
    .GetNewsCollection()
    .Aggregate()
    .SortByDescending(f => f.PublishDate)
    .Limit(20);

    var result = await ProjectToEntry(agg).ToListAsync();

    return result;
});

app.Run();

static IAggregateFluent<Entry> ProjectToEntry(IAggregateFluent<NewsEntity> agg) =>
    agg.Project(Builders<NewsEntity>.Projection.Expression(f => new Entry(
        f.Title,
        f.Description,
        f.PublishDate,
        f.Links,
        f.Source
        )));

public record Entry(string Title, string Description, DateTime PublishDate, NewsLinkEntity[] Links, string Source);