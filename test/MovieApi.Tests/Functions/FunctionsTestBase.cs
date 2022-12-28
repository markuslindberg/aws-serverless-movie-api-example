using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using MovieApi.Logging;
using Serilog;
using Xunit.Abstractions;
using System.Text.Json;

namespace MovieApi.Tests.Functions;

public abstract class FunctionsTestBase : IAsyncLifetime
{
    protected readonly IServiceProvider _serviceProvider;
    protected readonly ITestOutputHelper _output;
    protected readonly TestLambdaContext _context;

    protected FunctionsTestBase(ITestOutputHelper output)
    {
        var services = Startup.Configure();

        services.AddSingleton<ILogger>(new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.TestOutput(output, Serilog.Events.LogEventLevel.Verbose)
            .Enrich.With<AwsLambdaContextEnricher>()
            .Enrich.FromLogContext()
            .CreateLogger());

        _serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });
        _context = new TestLambdaContext();
        _output = output;
    }

    public async Task InitializeAsync()
    {
        var jsons = await File.ReadAllTextAsync("../../../TestData/MoviesTable-requests.json");
        var requests = JsonSerializer.Deserialize<List<Amazon.DynamoDBv2.Model.WriteRequest>>(jsons);
        var client = _serviceProvider.GetRequiredService<IAmazonDynamoDB>();
        var tableName = Environment.GetEnvironmentVariable("TABLE_NAME") ?? "MoviesTable-dev";
        await client.BatchWriteItemAsync(new Dictionary<string, List<Amazon.DynamoDBv2.Model.WriteRequest>>
        {
            {tableName, requests}
        });
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected APIGatewayProxyRequest CreateRequestWithPathParams(string key, string value)
    {
        return CreateRequestWithPathParams(new Dictionary<string, string> { { key, value } });
    }

    protected APIGatewayProxyRequest CreateRequestWithPathParams(Dictionary<string, string> parameters)
    {
        return new APIGatewayProxyRequest
        {
            PathParameters = parameters
        };
    }

    protected APIGatewayProxyRequest CreateRequestWithQueryParams(string key, string value)
    {
        return CreateRequestWithQueryParams(new Dictionary<string, string> { { key, value } });
    }

    protected APIGatewayProxyRequest CreateRequestWithQueryParams(Dictionary<string, string> parameters)
    {
        return new APIGatewayProxyRequest
        {
            QueryStringParameters = parameters
        };
    }
}