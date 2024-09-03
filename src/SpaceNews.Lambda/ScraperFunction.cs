using Amazon.Lambda.Core;
using AWS.Lambda.Powertools.Logging;
using Microsoft.Extensions.DependencyInjection;
using SpaceNews.Scraper;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SpaceNews.Lambda;

public class ScraperFunction
{
    private readonly ServiceProvider _serviceProvider;

    public ScraperFunction()
    {
        _serviceProvider = ConfigureServices();
    }

    public ScraperFunction(ServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public async Task FunctionHandler(ILambdaContext context)
    {
        using var scope = _serviceProvider.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<ISpaceNewsProcessor>();

        await processor.Process();
    }

    private static ServiceProvider ConfigureServices()
    {
        var serviceCollection = new ServiceCollection();
        
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
            ?? throw new ArgumentNullException("DB_CONNECTION_STRING");
        
        var _logger = Logger.Create<SpaceNewsProcessor>();
        var processor = new SpaceNewsProcessor(connectionString, _logger);
        serviceCollection.AddSingleton<ISpaceNewsProcessor>(processor);        

        return serviceCollection.BuildServiceProvider();
    }
}