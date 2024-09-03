using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using SpaceNews.Summary;

namespace SpaceNews.Lambda;

public class SummaryFunction
{
    private readonly ServiceProvider _serviceProvider;

    public SummaryFunction()
    {
        _serviceProvider = ConfigureServices();
    }

    public SummaryFunction(ServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task FunctionHandler(ILambdaContext context)
    {
        using var scope = _serviceProvider.CreateScope();
        var summaryGenerator = scope.ServiceProvider.GetRequiredService<ISummaryGenerator>();

        await summaryGenerator.Generate();
    }

    private static ServiceProvider ConfigureServices()
    {
        var serviceCollection = new ServiceCollection();

        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
            ?? throw new ArgumentNullException("DB_CONNECTION_STRING");
        var modelId = Environment.GetEnvironmentVariable("MODEL_ID")
            ?? throw new ArgumentNullException("MODEL_ID");

        var summaryGenerator = new SummaryGenerator(modelId, connectionString);
        serviceCollection.AddSingleton<ISummaryGenerator>(summaryGenerator);

        return serviceCollection.BuildServiceProvider();
    }
}
