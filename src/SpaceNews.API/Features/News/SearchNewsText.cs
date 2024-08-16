using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver.Search;
using MongoDB.Driver;
using SpaceNews.Shared.Database.Model;

namespace SpaceNews.API.Features.News;

public record SearchNewsTextRequest(string Search, string[]? Sources);

[HttpGet("/news/text")]
[AllowAnonymous]
public class SearchNewsText(IMongoDatabase db) : Endpoint<SearchNewsTextRequest, IList<Entry>>
{
    public override async Task<IList<Entry>> ExecuteAsync(SearchNewsTextRequest req, CancellationToken ct)
    {
        var sources = req.Sources;
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
              Builders<NewsEntity>.Search.Text(x => x.Title, req.Search, fuzzyOptions),
              indexName: "news_text_index"
            )
            .Match(f => sources == null || sources.Length == 0 || sources.Contains(f.SourceId))
            .SortByDescending<NewsEntity>(x => x.PublishDate);

        return await agg.ProjectToEntry().ToListAsync(ct);
    }
}