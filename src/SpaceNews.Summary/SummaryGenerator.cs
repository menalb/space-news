using MongoDB.Driver;
using SpaceNews.Shared.Database.Model;
using System.Text.RegularExpressions;
using System.Text;
using System.Web;
using Microsoft.SemanticKernel;

namespace SpaceNews.Summary
{
    public interface ISummaryGenerator
    {
        Task Generate(CancellationToken cancellationToken = default);
    }
    public class SummaryGenerator : ISummaryGenerator
    {
        private readonly IMongoDatabase _database;
        private readonly KernelPlugin _kernelPlugin;
        private readonly Kernel _kernel;
        public SummaryGenerator(Kernel kernel, IMongoClient mongoClient)
        {
            _database = mongoClient.GetDatabase("SpaceNews");
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));

            var path = Path.Combine(Environment.CurrentDirectory, "Plugins");
            _kernelPlugin = kernel.ImportPluginFromPromptDirectory(path);
        }
        public async Task Generate(CancellationToken cancellationToken = default)
        {
            var fromWhen = DateTime.Now.AddDays(-1);

            var today = await GetLatestNews(fromWhen);

            var summary = await GenerateSummary(today, cancellationToken);

            if (!string.IsNullOrEmpty(summary))
            {
                await Store(fromWhen, summary);
            }
        }

        private async Task<string> GenerateSummary(IList<NewsEntity> news, CancellationToken cancellationToken = default)
        {
            var prompt = PreparePrompt(news);

            var userMessage = @$"{prompt}
From the above list of news create a simple and linear summarization can can be quickly read as a single paragraph, not as a list. Make sure to include each news in the summary.";

            var response = await _kernel.InvokeAsync(_kernelPlugin["Summarize"], new() { ["input"] = prompt });

            Console.WriteLine(response);

            return response.ToString();
        }

        private string PreparePrompt(IList<NewsEntity> news)
        {
            var _htmlRegex = new Regex("<.*?>", RegexOptions.Compiled);
            var sb = new StringBuilder();
            news.ToList().ForEach(n =>
            {
                sb.Append(@$"####
title: {n.Title.Trim()}
description: {_htmlRegex.Replace(n.Description, "").Trim()}
");
            });

            return HttpUtility.HtmlDecode(sb.ToString());
        }

        private async Task Store(DateTime fromWhen, string summary)
        {
            var entity = new SummaryEntity
            {
                DateTime = DateTime.UtcNow,
                Summary = summary
            };
            var coll = _database.GetSummariesCollection();
            await coll.InsertOneAsync(entity);
        }

        private async Task<IList<NewsEntity>> GetLatestNews(DateTime fromWhen)
        {
            var sourcesToExclude = await _database
                .GetSourcesCollection()
                .Find(s => s.ExcludeFromSummary)
                .Project(s => s.Id)
                .ToListAsync();
            var toExclude = sourcesToExclude.ToList() ?? [];

            return await _database
                .GetNewsCollection()
                .Find(n => n.PublishDate > fromWhen && !toExclude.Contains(n.SourceId))
                .SortByDescending(n => n.PublishDate)
                .ToListAsync();
        }
    }
}
