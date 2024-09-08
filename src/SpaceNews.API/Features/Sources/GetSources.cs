using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;
using SpaceNews.Shared.Database.Model;

namespace SpaceNews.API.Features.Sources;

[HttpGet("/sources")]
[AllowAnonymous]
public class GetNews(IMongoDatabase db) : EndpointWithoutRequest<IList<SourceEntity>>
{
    public override async Task<IList<SourceEntity>> ExecuteAsync(CancellationToken ct)
        => await db
        .GetSourcesCollection()
        .Aggregate()
        .Match(s=>s.IsActive)
        .SortBy(s => s.Name).ToListAsync(cancellationToken: ct);   
}