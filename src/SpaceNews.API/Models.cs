using SpaceNews.Shared.Database.Model;

public record Entry(string Title, string Description, DateTime PublishDate, NewsLinkEntity[] Links, string Source);