using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Search;
using SpaceNews.Shared.Database.Model;
using SmartComponents.LocalEmbeddings;

namespace SpaceNews.API.Features.News;

public record SearchNewsSemanticRequest(string Search, string[]? Sources);

[HttpGet("/news/semantic")]
[AllowAnonymous]
public class SearchNewsSemantic(IMongoDatabase db, LocalEmbedder embedder)
    : Endpoint<SearchNewsSemanticRequest, IList<Entry>>
{
    public override async Task<IList<Entry>> ExecuteAsync(SearchNewsSemanticRequest req, CancellationToken ct)
    {
        var sources = req.Sources;
        var target = embedder.Embed(req.Search);
        var options = new VectorSearchOptions<NewsEntity>()
        {
            IndexName = "news_vector_index",
            NumberOfCandidates = 150,
            Filter = Builders<NewsEntity>.Filter.Where(f =>
                sources == null || sources.Length == 0 || sources.Contains(f.SourceId))
        };
    
        var agg = db
            .GetNewsCollection()
            .Aggregate()
            .VectorSearch(m => m.Embeddings, target.Values, 10, options);
    
        return await agg.ProjectToEntry().ToListAsync(ct);
    }
}