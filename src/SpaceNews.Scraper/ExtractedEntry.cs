using SpaceNews.Shared.Database.Model;

namespace SpaceNews.Scraper;

public class ExtractedEntry
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required DateTime PublishDate { get; set; }
    public required NewsLinkEntity[] Links { get; set; }
    public string? ItemId { get; set; }
}


