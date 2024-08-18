using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;
using SpaceNews.Shared.Database.Model;

namespace SpaceNews.API.Features.News;

public record NewsRequest(string[]? Sources);

[HttpGet("/news")]
[AllowAnonymous]
public class GetNews(IMongoDatabase db) : Endpoint<NewsRequest, IList<Entry>>
{
    public override async Task<IList<Entry>> ExecuteAsync(NewsRequest req, CancellationToken ct)
    {
        var sources = req.Sources;
        var agg = db
            .GetNewsCollection()
            .Aggregate()
            .Match(f => sources == null || sources.Length == 0 || sources.Contains(f.SourceId))
            .SortByDescending(f => f.PublishDate)
            .Limit(50);

        return await agg.ProjectToEntry().ToListAsync(ct);
    }
}