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
    private readonly ITestOutputHelper _output;
    protected IServiceProvider _serviceProvider;
    protected IHttpClientFactory _clientFactory;
    
    protected FunctionsTestBase(ITestOutputHelper output)
    {
        _output = output;
    }

    public async Task InitializeAsync()
    {
        if (!Uri.TryCreate(Environment.GetEnvironmentVariable("API_ENDPOINT"), new UriCreationOptions(), out var endpoint) &&
            !Uri.TryCreate(await GetAwsStackApiEndpoint(), new UriCreationOptions(), out endpoint))
            throw new Exception("API_ENDPOINT not set");

        var services = Startup.Configure();

        services
            .AddTransient<AwsSignatureHandler>()
            .AddTransient(_ => new AwsSignatureHandlerSettings(
                Environment.GetEnvironmentVariable("AWS_REGION") ?? "eu-north-1",
                "execute-api",
                GetAwsCredentials()));

        services
            .AddHttpClient("aws-client", client => client.BaseAddress = endpoint)
            .AddHttpMessageHandler<AwsSignatureHandler>();

        _serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });
        _clientFactory = _serviceProvider.GetRequiredService<IHttpClientFactory>();

        await InitializeDynamoDb();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    private async Task InitializeDynamoDb()
    {
        _output.WriteLine("FunctionsTestBase InitializeDynamoDb");

        var jsons = await File.ReadAllTextAsync("../../../TestData/MoviesTable-requests.json");
        var requests = JsonSerializer.Deserialize<List<Amazon.DynamoDBv2.Model.WriteRequest>>(jsons);
        var client = _serviceProvider.GetRequiredService<IAmazonDynamoDB>();
        var tableName = Environment.GetEnvironmentVariable("TABLE_NAME") ?? "MoviesTable-dev";

        await client.BatchWriteItemAsync(new Dictionary<string, List<Amazon.DynamoDBv2.Model.WriteRequest>>
        {
            {tableName, requests}
        });

        _output.WriteLine("FunctionsTestBase InitializeDynamoDb completed");
    }

    public static async Task<string> GetAwsStackApiEndpoint()
    {
        var client = new AmazonCloudFormationClient(new AmazonCloudFormationConfig
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(Environment.GetEnvironmentVariable("AWS_REGION") ?? "eu-north-1")
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