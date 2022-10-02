using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using MovieApi.Logging;
using Serilog;
using Xunit.Abstractions;

namespace MovieApi.Tests.Functions;

public abstract class FunctionsTestBase
{
    protected readonly IServiceProvider _serviceProvider;
    protected readonly ITestOutputHelper _output;
    protected readonly TestLambdaContext _context;

    protected FunctionsTestBase(ITestOutputHelper output)
    {
        var serviceContainer = Startup.Configure();

        serviceContainer.AddSingleton<ILogger>(new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.TestOutput(output, Serilog.Events.LogEventLevel.Verbose)
            .Enrich.With<AwsLambdaContextEnricher>()
            .Enrich.FromLogContext()
            .CreateLogger());

        serviceContainer.AddSingleton<IAmazonDynamoDB>(new AmazonDynamoDBClient(
            new AmazonDynamoDBConfig
            {
                ServiceURL = "http://localhost:8000/"
            }));

        _serviceProvider = serviceContainer.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });
        _context = new TestLambdaContext();
        _output = output;
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