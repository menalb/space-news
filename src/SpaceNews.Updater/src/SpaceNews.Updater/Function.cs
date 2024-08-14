using Amazon.Lambda.Core;
using MongoDB.Driver;
using SmartComponents.LocalEmbeddings;
using SpaceNews.Scraper;
using SpaceNews.Shared.Database.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SpaceNews.Updater;

public class Function
{
    
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public async Task FunctionHandler(ILambdaContext context)
    {
        // var connectionString = config.GetConnectionString("SpaceNews");
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

        var processor = new SpaceNewsProcessor(connectionString ?? throw new ArgumentNullException(nameof(connectionString)));
        await processor.Process();
    }
}
