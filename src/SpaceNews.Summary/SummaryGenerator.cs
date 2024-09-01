using Amazon.BedrockRuntime.Model;
using Amazon.BedrockRuntime;
using MongoDB.Driver;
using SpaceNews.Shared.Database.Model;
using System.Text.RegularExpressions;
using System.Text;
using System.Web;

namespace SpaceNews.Summary
{
    public interface ISummaryGenerator
    {
        Task Generate();
    }
    public class SummaryGenerator : ISummaryGenerator
    {
        private readonly string _modelId;
        private readonly IMongoDatabase _database;
        public SummaryGenerator(string modelId, string connectionString)
        {
            _modelId = modelId;
            var conn = new MongoClient(connectionString);
            _database = conn.GetDatabase("SpaceNews");
        }
        public async Task Generate()
        {
            var fromWhen = DateTime.Now.AddDays(-1);

            var today = await GetLatestNews(fromWhen);

            var summary = await GenerateSummary(today);

            if (!string.IsNullOrEmpty(summary))
            {
                await Store(fromWhen, summary);
            }
        }

        private async Task<string> GenerateSummary(IList<NewsEntity> news)
        {
            var prompt = PreparePrompt(news);

            var userMessage = @$"{prompt}
From the above list of news create a simple and linear summarization can can be quickly read as a single paragraph, not as a list. Make sure to include each news in the summary.";

            var request = new ConverseRequest
            {
                ModelId = _modelId,
                Messages =
                [
                    new() {
                        Role = ConversationRole.User,
                        Content = new List<ContentBlock> { new ContentBlock { Text = userMessage } }
                    }
                ],
                InferenceConfig = new InferenceConfiguration()
                {
                    MaxTokens = 4096,
                    Temperature = 0.5F,
                    TopP = 1F
                }
            };

            try
            {
                // TODO: DI
                using var client = new AmazonBedrockRuntimeClient(Amazon.RegionEndpoint.USEast1);

                // Send the request to the Bedrock Runtime and wait for the result.
                var response = await client.ConverseAsync(request);

                // Extract and print the response text.
                return response?.Output?.Message?.Content?[0]?.Text ?? "";
            }
            catch (AmazonBedrockRuntimeException e)
            {
                Console.WriteLine($"ERROR: Can't invoke '{_modelId}'. Reason: {e.Message}");
                throw;
            }
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
