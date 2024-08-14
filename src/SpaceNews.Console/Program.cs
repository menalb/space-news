using Microsoft.Extensions.Configuration;
using SpaceNews.Scraper;

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile($"appsettings.json")
    .Build();

var connectionString = config.GetConnectionString("SpaceNews");

var processor = new SpaceNewsProcessor(connectionString ?? throw new ArgumentNullException(nameof(connectionString)));
await processor.Process();