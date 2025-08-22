using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Search;
using SpaceNews.Shared.Database.Model;
using SmartComponents.LocalEmbeddings;

namespace SpaceNews.API.Features.News;

[HttpGet("/news/hybrid")]
[AllowAnonymous]
public class SearchNewsHybrid(IMongoDatabase db, LocalEmbedder embedder)
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

        var aggSemantic = db
            .GetNewsCollection()
            .Aggregate()
            .VectorSearch(m => m.Embeddings, target.Values, 10, options);


        var fuzzyOptions = new SearchFuzzyOptions()
        {
            MaxEdits = 1,
            PrefixLength = 1,
            MaxExpansions = 256
        };

        var aggText = db
            .GetNewsCollection()
            .Aggregate()
            .Search(
                Builders<NewsEntity>.Search.Text(x => x.Title, req.Search, fuzzyOptions),
                indexName: "news_text_index"
            )
            .Match(f => sources == null || sources.Length == 0 || sources.Contains(f.SourceId))
            .SortByDescending(x => x.PublishDate);

        var s = aggSemantic.ToListAsync(ct);
        var t = aggText.ToListAsync(ct);

        await Task.WhenAll([s, t]);
        
        if (s is null || t is null) return [];

        var u = ReciprocalRankFusion(s.Result, t.Result, weightA: 2, weightB: 1, k: 60);
        return u.ProjectToEntry();
    }

    private List<NewsEntity> ReciprocalRankFusion(
        IList<NewsEntity> listA,
        IList<NewsEntity> listB,
        double weightA = 1.0,
        double weightB = 1.0,
        int k = 60
    )
    {
        var rankA = listA.Select((item, idx) => (item.Id, rank: idx + 1)).ToDictionary(x => x.Id, x => x.rank);
        var rankB = listB.Select((item, idx) => (item.Id, rank: idx + 1)).ToDictionary(x => x.Id, x => x.rank);

        var all = listA.Concat(listB).GroupBy(x => x.Id).Select(x => x.First()).ToList();

        // Compute RRF scores
        var scores = new Dictionary<ObjectId, double>(all.Count);

        foreach (var doc in all)
        {
            double s = 0;

            if (rankA.TryGetValue(doc.Id, out var rA))
                s += weightA * (1.0 / (k + rA));

            if (rankB.TryGetValue(doc.Id, out var rB))
                s += weightB * (1.0 / (k + rB));

            scores[doc.Id] = s;
        }

        // Sort:
        // 1) RRF score (desc)
        // 2) Best (lowest) rank across lists (asc) — nice stable tie-breaker
        // 3) Newer publish date (desc) — domain-friendly secondary tie-breaker
        int BestRank(ObjectId id)
        {
            var a = rankA.GetValueOrDefault(id, int.MaxValue);
            var b = rankB.GetValueOrDefault(id, int.MaxValue);
            return Math.Min(a, b);
        }

        return all.OrderByDescending(d => scores[d.Id])
            .ThenBy(d => BestRank(d.Id))
            .ThenByDescending(d => d.PublishDate)
            .ToList();
    }
}

// var p1 = new EmptyPipelineDefinition<NewsEntity>().VectorSearch(m => m.Embeddings, target.Values, 10, options);
// var fuzzyOptions = new SearchFuzzyOptions()
// {
//     MaxEdits = 1,
//     PrefixLength = 1,
//     MaxExpansions = 256
// };
//
// var p2 = new EmptyPipelineDefinition<NewsEntity>().Search(
//     Builders<NewsEntity>.Search.Text(x => x.Title, req.Search, fuzzyOptions),
//     indexName: "news_text_index"
// );
// var weights = new Dictionary<string, double>()
// {
//     { "vectorPipeline", 0.5 },
//     { "fullTextPipeline", 0.5 }
// };
// var pipelines = new Dictionary<string, PipelineDefinition<NewsEntity, NewsEntity>>
//     { { "vectorPipeline", p1 }, { "fullTextPipeline", p2 } };
//
// var agg2 = db.GetNewsCollection()
//     .Aggregate()
//     .RankFusion(pipelines,
//         weights,
//         new RankFusionOptions<NewsEntity>()
//         {
//             ScoreDetails = true
//         }
//     );