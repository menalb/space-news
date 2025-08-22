using MongoDB.Driver;
using SpaceNews.Shared.Database.Model;

namespace SpaceNews.API;

internal static class Extensions
{
    internal static IAggregateFluent<Entry> ProjectToEntry(this IAggregateFluent<NewsEntity> agg) =>
        agg.Project(Builders<NewsEntity>.Projection.Expression(f => new Entry(
            f.Title,
            f.Description,
            f.PublishDate,
            f.Links,
            f.Source
        )));

    internal static IList<Entry> ProjectToEntry(this IList<NewsEntity> agg) =>
        agg.Select(f => new Entry(
            f.Title,
            f.Description,
            f.PublishDate,
            f.Links,
            f.Source
        )).ToList();
}