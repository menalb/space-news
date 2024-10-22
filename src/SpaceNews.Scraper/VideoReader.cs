using SmartComponents.LocalEmbeddings;
using SpaceNews.Shared.Database.Model;

namespace SpaceNews.Scraper;

public class VideoReader(HttpClient httpClient, string apiKey)
{
    private const int maxResults = 100;
    public async Task<IEnumerable<NewsEntity>> GetVideoData(DateTime FromDate, SourceEntity feed, LocalEmbedder embedder)
    {
        var channelId = feed.ChannelId;
        var publishedAfter = FromDate.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK");
        var response = await httpClient.GetAsync($"https://www.googleapis.com/youtube/v3/search?key={apiKey}&part=id,snippet&channelId={channelId}&maxResults={maxResults}&order=date&type=video&publishedAfter={publishedAfter}");

        response.EnsureSuccessStatusCode();

        var result = System.Text.Json.JsonSerializer.Deserialize<Root>(await response.Content.ReadAsStringAsync());

        if(result is not null)
        {
            return Map(result, feed, embedder);
        }
        return [];
    }

    private IEnumerable<NewsEntity> Map(Root result, SourceEntity source, LocalEmbedder embedder)
    {
        EmbeddingF32 GenerateEmbedding(Snippet snippet)
        {
            var s = $"### Title: {snippet.title} ### Description: {snippet.description}";
            return embedder.Embed(s);
        }

        return result.items.Where(item => item is not null && item.snippet is not null).Select(item =>
        {
            var entry = new NewsEntity
            {
                Title = item.snippet.title ?? "",
                FeedItemId = item.id.videoId,
                PublishDate = item.snippet.publishedAt,
                Description = item.snippet.description ?? "",
                SourceId = source.Id,
                Source = source.Name,
                Links = [
                    new NewsLinkEntity{Title = "Thumbnails High", Uri= item.snippet?.thumbnails?.high.url ?? "" },
                    new NewsLinkEntity{Title = "Thumbnails Medium", Uri= item.snippet?.thumbnails?.medium.url ?? ""}
                ],
                Embeddings = GenerateEmbedding(item.snippet).Values.ToArray(),
            };
            return entry;
        });
    }
}
public class Default
{
    public required string url { get; set; }
    public int width { get; set; }
    public int height { get; set; }
}

public class High
{
    public required string url { get; set; }
    public int width { get; set; }
    public int height { get; set; }
}

public class Id
{
    public string kind { get; set; } = "";
    public string videoId { get; set; } = "";
}

public class Item
{
    public string kind { get; set; } = "";
    public string etag { get; set; } = "";
    public required Id id { get; set; }
    public required Snippet snippet { get; set; }
}

public class Medium
{
    public string url { get; set; } = "";
    public int width { get; set; }
    public int height { get; set; }
}

public class PageInfo
{
    public int totalResults { get; set; }
    public int resultsPerPage { get; set; }
}

public class Root
{
    public string kind { get; set; } = "";
    public string etag { get; set; } = "";
    public string nextPageToken { get; set; } = "";
    public string regionCode { get; set; } = "";
    public PageInfo? pageInfo { get; set; }
    public List<Item> items { get; set; } = [];
}

public class Snippet
{
    public DateTime publishedAt { get; set; }
    public required string channelId { get; set; }
    public string title { get; set; } = "";
    public string description { get; set; } = "";
    public Thumbnails? thumbnails { get; set; }
    public string channelTitle { get; set; } = "";
    public string liveBroadcastContent { get; set; } = "";
    public DateTime publishTime { get; set; }
}

public class Thumbnails
{
    public Default @default { get; set; }
    public Medium medium { get; set; }
    public High high { get; set; }
}


