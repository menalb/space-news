using MongoDB.Driver;
using SmartComponents.LocalEmbeddings;
using SpaceNews.Shared.Database.Model;

var builder = WebApplication.CreateBuilder(args);

var SpaceNewsApiOriginPolicy = "_spaceNewsApiOriginPolicy";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: SpaceNewsApiOriginPolicy,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:5173");
                      });
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

app.MapGet("/news", async (string text, IMongoDatabase db) =>
{
    using var embedder = new LocalEmbedder();
    var target = embedder.Embed(text);
    var options = new VectorSearchOptions<NewsEntity>()
    {
        IndexName = "news_vector_index",
        NumberOfCandidates = 150
    };

    var agg = db
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