using Amazon;
using Amazon.CloudFormation;
using Amazon.CloudFormation.Model;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using AwsSignatureVersion4;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Xunit.Abstractions;

namespace MovieApi.Tests.Functions;

public abstract class FunctionsTestBase : IAsyncLifetime
{
    protected readonly IServiceProvider _serviceProvider;
    protected readonly IHttpClientFactory _clientFactory;
    private readonly ITestOutputHelper _output;

    protected FunctionsTestBase(ITestOutputHelper output)
    {
        _output = output;
        _output.WriteLine("FunctionsTestBase constructor");

        var services = Startup.Configure();

        var credentials = GetAwsCredentials();

        var endpoint = Environment.GetEnvironmentVariable("API_ENDPOINT") ??
            GetApiEndpoint().GetAwaiter().GetResult() ??
            throw new Exception("API_ENDPOINT not set");

        _output.WriteLine($"FunctionsTestBase API_ENDPOINT: {endpoint}");

        services
            .AddTransient<AwsSignatureHandler>()
            .AddTransient(_ => new AwsSignatureHandlerSettings(
                Environment.GetEnvironmentVariable("DEV_REGION") ?? "eu-north-1",
                "execute-api",
                credentials));

        services
            .AddHttpClient("aws-client", client => client.BaseAddress = new Uri(endpoint))
            .AddHttpMessageHandler<AwsSignatureHandler>();

        _serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });
        _clientFactory = _serviceProvider.GetRequiredService<IHttpClientFactory>();
    }

    public async Task InitializeAsync()
    {
        _output.WriteLine("FunctionsTestBase InitializeAsync starting");
        var jsons = await File.ReadAllTextAsync("../../../TestData/MoviesTable-requests.json");
        var requests = JsonSerializer.Deserialize<List<Amazon.DynamoDBv2.Model.WriteRequest>>(jsons);
        var client = _serviceProvider.GetRequiredService<IAmazonDynamoDB>();
        var tableName = Environment.GetEnvironmentVariable("TABLE_NAME") ?? "MoviesTable-dev";
        await client.BatchWriteItemAsync(new Dictionary<string, List<Amazon.DynamoDBv2.Model.WriteRequest>>
        {
            {tableName, requests}
        });
        _output.WriteLine("FunctionsTestBase InitializeAsync completed");
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    private static async Task<string> GetApiEndpoint()
    {
        var client = new AmazonCloudFormationClient(new AmazonCloudFormationConfig
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(Environment.GetEnvironmentVariable("DEV_REGION") ?? "eu-north-1")
        });

        var stacks = await client.DescribeStacksAsync(new DescribeStacksRequest
        {
            StackName = Environment.GetEnvironmentVariable("DEV_STACK_NAME") ?? "MovieApi-dev"
        });

        return stacks.Stacks[0].Outputs
            .Where(o => o.OutputKey == "WebEndpoint")
            .Select(o => o.OutputValue)
            .FirstOrDefault();
    }

    private static ImmutableCredentials GetAwsCredentials()
    {
        var chain = new CredentialProfileStoreChain();
        if (chain.TryGetAWSCredentials("default", out var awsCredentials))
        {
            return awsCredentials.GetCredentials();
        }
        else
        {
            return new EnvironmentVariablesAWSCredentials().GetCredentials();
        }
    }
}