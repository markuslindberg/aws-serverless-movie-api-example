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

    protected FunctionsTestBase(ITestOutputHelper output)
    {
        var services = Startup.Configure();

        services
            .AddTransient<AwsSignatureHandler>()
            .AddTransient(_ => new AwsSignatureHandlerSettings(
                Environment.GetEnvironmentVariable("DEV_REGION") ?? "eu-north-1",
                "execute-api",
                GetAwsCredentials()));

        services
            .AddHttpClient("aws-client", client =>
                client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("API_ENDPOINT") ?? throw new Exception("API_ENDPOINT not set")))
            .AddHttpMessageHandler<AwsSignatureHandler>();

        _serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });
        _clientFactory = _serviceProvider.GetRequiredService<IHttpClientFactory>();
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

    private static ImmutableCredentials GetAwsCredentials()
    {
        var chain = new CredentialProfileStoreChain();
        if (chain.TryGetAWSCredentials("default", out var awsCredentials))
        {
            return awsCredentials.GetCredentials();
        }
        else
        {
            throw new Exception("Could not find AWS credentials");
        }
    }
}