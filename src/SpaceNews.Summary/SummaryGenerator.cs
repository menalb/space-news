using MongoDB.Driver;
using SpaceNews.Shared.Database.Model;
using System.Text.RegularExpressions;
using System.Text;
using System.Web;
using Microsoft.SemanticKernel;
using System.Text.Json;

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

            var summaryResult = await GenerateSummary(today, cancellationToken);

            if (!string.IsNullOrEmpty(summaryResult))
            {
                var (summary, parsed) = ParseSummary(summaryResult);
                await Store(fromWhen, summary, parsed);
            }
        }

        private async Task<string> GenerateSummary(IList<NewsEntity> news, CancellationToken cancellationToken = default)
        {
            var prompt = PreparePrompt(news);

            //            var userMessage = @$"{prompt}
            //From the above list of news create a simple and linear summarization can can be quickly read as a single paragraph, not as a list. Make sure to include each news in the summary.";

            var userMessage = @$"Given this list of news, convert it into a JSON format where each entry has a summarized version of the 'Description' and 'Title' field as 'summary' and the corresponding 'Id' field as 'id'. Ensure the summaries are concise but capture the main point of each news. Output the result as a JSON array. If two or more news are similar, they should be reported only once and the id field should be an array with all the cited news Ids
{prompt}";

            var ka = new KernelArguments(new PromptExecutionSettings
            {
                ExtensionData = new Dictionary<string, object> { { "response_format", "json_object" } }
            });

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
Id: {n.Id}
Title: {n.Title.Trim()}
Description: {_htmlRegex.Replace(n.Description, "").Trim()}
PublishDate: {n.PublishDate}
");
            });

            return HttpUtility.HtmlDecode(sb.ToString());
        }

        private (string summary, SummaryPart[] parts) ParseSummary(string message)
        {
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            var result = JsonSerializer.Deserialize<SummarizedResult>(message, serializeOptions);

            if (result is not null && result.Summaries is not null)
            {
                var summary = string.Join(" ", result.Summaries.Select(r => r.Summary));
                return (summary, result.Summaries.Select(p => new SummaryPart
                {
                    NewsId = p.Ids,
                    Summary = p.Summary
                }).ToArray());
            }
            throw new Exception($"Unable to parse: {message}");
        }

        private async Task Store(DateTime fromWhen, string summary, SummaryPart[] parts)
        {
            var entity = new SummaryEntity
            {
                DateTime = DateTime.UtcNow,
                Summary = summary,
                SummaryParts = parts
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

record SummarizedResult(SummarizedPart[] Summaries);
record SummarizedPart(string[] Ids, string Summary);
