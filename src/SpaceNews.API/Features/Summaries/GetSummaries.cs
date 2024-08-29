using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;
using SpaceNews.Shared.Database.Model;

namespace SpaceNews.API.Features.Summaries;

[HttpGet("/summary")]
[AllowAnonymous]
public class GetLastSummary(IMongoDatabase db) : EndpointWithoutRequest<SummaryEntity>
{
    public override async Task<SummaryEntity> ExecuteAsync(CancellationToken ct)
        => await db
        .GetSummariesCollection()
        .Find(s => s.DateTime >= DateTime.UtcNow.AddDays(-1))
        .SortByDescending(s => s.DateTime)
        .FirstOrDefaultAsync(ct);
}