using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpaceNews.Scraper;
using SpaceNews.Summary;

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile($"appsettings.json")
    .Build();

var connectionString = config.GetConnectionString("SpaceNews");

using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = factory.CreateLogger<SpaceNewsProcessor>();

var processor = new SpaceNewsProcessor(
    connectionString ?? throw new ArgumentNullException(nameof(connectionString)),
    logger
    );
await processor.Process();

var summaryGenerator = new SummaryGenerator("amazon.titan-text-lite-v1", connectionString);
await summaryGenerator.Generate();