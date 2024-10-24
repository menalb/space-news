using SpaceNews.Shared.Database.Model;
using System.ServiceModel.Syndication;
using System.Xml;

namespace SpaceNews.Scraper;

public interface IFeedReader
{
    Task<IEnumerable<ExtractedEntry>> GetFeed(string feedUrl);
}
public sealed record FeedLink(string Title, Uri Uri);
public record ParsedFeed(string Id, string Title, string Description, DateTimeOffset PublishDate, FeedLink[] Links);
public class FeedReader(HttpClient httpClient) : IFeedReader
{
    public async Task<IEnumerable<ExtractedEntry>> GetFeed(string feedUrl)
    {
        string rssContent = await DownloadRssFeedAsync(feedUrl);
        var rssDoc = LoadRssFeed(rssContent);
        return MapFeeds(ParseRssFeed(rssDoc));
    }
    private static IEnumerable<ExtractedEntry> MapFeeds(IEnumerable<ParsedFeed> result) =>
        result.Select(r => new ExtractedEntry
        {
            Title = r.Title,
            Description = r.Description,
            PublishDate = r.PublishDate.UtcDateTime,
            Links = r.Links.Select(fl => new NewsLinkEntity { Uri = fl.Uri.ToString(), Title = fl.Title }).ToArray(),
            ItemId = r.Id
        });

    private IEnumerable<ParsedFeed> ParseRssFeed(SyndicationFeed feed) =>
        feed.Items.Select(item =>
        new ParsedFeed(
            item.Id,
            item.Title.Text,
            item.Summary.Text,
            item.PublishDate,
            item.Links.Select(l => new FeedLink(l.Title, l.Uri)).ToArray())
        );

    private async Task<string> DownloadRssFeedAsync(string url)
    {
        httpClient.BaseAddress = new Uri(url);
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

        return await httpClient.GetStringAsync(url);
    }

    private SyndicationFeed LoadRssFeed(string rssContent)
    {
        using XmlReader reader = XmlReader.Create(new StringReader(rssContent));
        return SyndicationFeed.Load(reader);
    }
}
