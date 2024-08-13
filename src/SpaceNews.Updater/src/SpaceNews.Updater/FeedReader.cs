using System.ServiceModel.Syndication;
using System.Xml;

namespace SpaceNews.Updater;

public record FeedLink(string Title, Uri Uri);
public record ParsedFeed(string Id, string Title, string Description, DateTimeOffset PublishDate, FeedLink[] Links);
public class FeedReader(HttpClient httpClient)
{
    public async Task<IEnumerable<ParsedFeed>> GetFeed(string feedUrl)
    {
        string rssContent = await DownloadRssFeedAsync(feedUrl);
        var rssDoc = LoadRssFeed(rssContent);
        return ParseRssFeed(rssDoc);
    }
    private IEnumerable<ParsedFeed> ParseRssFeed(SyndicationFeed feed)
    {
        return feed.Items.Select(item =>
        new ParsedFeed(
            item.Id,
            item.Title.Text,
            item.Summary.Text,
            item.PublishDate,
            item.Links.Select(l => new FeedLink(l.Title, l.Uri)).ToArray())
        );
    }

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
