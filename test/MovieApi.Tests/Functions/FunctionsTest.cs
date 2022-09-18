using System.Reflection;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MovieApi.Functions;
using MovieApi.Tests.Infrastructure;
using Serilog;
using Xunit.Abstractions;

namespace MovieApi.Tests.Functions;

[UsesVerify]
[TestCaseOrderer("MovieApi.Tests.Infrastructure.PriorityOrderer", "MovieApi.Tests")]
public class FunctionsTest : IAsyncLifetime
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TestLambdaContext _context;
    private readonly ITestOutputHelper _output;

    private readonly TestcontainersContainer _dynamoDbLocal = new TestcontainersBuilder<TestcontainersContainer>()
        .WithImage("amazon/dynamodb-local:1.18.0")
        .WithCommand("-jar", "DynamoDBLocal.jar", "-inMemory", "-sharedDb")
        .WithPortBinding(8000)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8000))
        .Build();

    private AmazonDynamoDBClient _dynamoDbLocalClient = new AmazonDynamoDBClient(
        new AmazonDynamoDBConfig
        {
            ServiceURL = "http://localhost:8000/"
        });

    public FunctionsTest(ITestOutputHelper output)
    {
        var serviceContainer = new ServiceCollection();
        serviceContainer.AddMediatR(typeof(Startup).GetTypeInfo().Assembly);
        serviceContainer.AddValidatorsFromAssemblyContaining(typeof(Startup));
        serviceContainer.AddSingleton<IAmazonDynamoDB>(_dynamoDbLocalClient);
        serviceContainer.AddSingleton<ILogger>(new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.TestOutput(output, Serilog.Events.LogEventLevel.Verbose)
            .CreateLogger());
        _serviceProvider = serviceContainer.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });
        _context = new TestLambdaContext();
        _output = output;
    }

    [Fact]
    [TestPriority(1)]
    public async Task GetMovieTest()
    {
        DynamoDbSetup(seed: true);

        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string> { { "movieId", "DieHard" } }
        };

        var function = new GetMovieFunction(_serviceProvider);
        var response = await function.HandleAsync(request, _context);

        await Verify(response);
    }

    [Fact]
    [TestPriority(2)]
    public async Task GetMoviesTest()
    {
        DynamoDbSetup(seed: true);

        var request = new APIGatewayProxyRequest
        {
            QueryStringParameters = new Dictionary<string, string> { { "category", "Action" } }
        };

        var function = new GetMoviesFunction(_serviceProvider);
        var response = await function.HandleAsync(request, _context);

        await Verify(response);
    }

    [Fact]
    [TestPriority(3)]
    public async Task CreateMovieTest()
    {
        DynamoDbSetup(seed: false);

        var request = new APIGatewayProxyRequest
        {
            Body = "{\"MovieId\": \"DieHard3\", \"Title\": \"Die Hard 3\", \"Year\": 2022, \"Category\": \"Action\", \"Budget\": \"Unlimited\", \"BoxOffice\": \"N/A\"}"
        };

        var function = new CreateMovieFunction(_serviceProvider);
        var response = await function.HandleAsync(request, _context);

        await Verify(response);
    }

    [Fact]
    [TestPriority(4)]
    public async Task UpdateMovieTest()
    {
        DynamoDbSetup(seed: true);

        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string> { { "movieId", "DieHard" } },
            Body = "{\"MovieId\": \"DieHard\", \"Title\": \"Die Hard\", \"Year\": 2022, \"Category\": \"Action\", \"Budget\": \"Unlimited\", \"BoxOffice\": \"N/A\"}"
        };

        var function = new UpdateMovieFunction(_serviceProvider);
        var response = await function.HandleAsync(request, _context);

        await Verify(response);
    }

    [Fact]
    [TestPriority(5)]
    public async Task DeleteMovieTest()
    {
        DynamoDbSetup(seed: true);

        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string> { { "movieId", "DieHard" } }
        };

        var function = new DeleteMovieFunction(_serviceProvider);
        var response = await function.HandleAsync(request, _context);

        await Verify(response);
    }

    [Fact]
    [TestPriority(6)]
    public async Task GetMovieCharactersTest()
    {
        DynamoDbSetup(seed: true);

        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string> { { "movieId", "DieHard" } }
        };

        var function = new GetMovieCharactersFunction(_serviceProvider);
        var response = await function.HandleAsync(request, _context);

        await Verify(response);
    }

    [Fact]
    [TestPriority(7)]
    public async Task GetMovieDirectorsTest()
    {
        DynamoDbSetup(seed: true);

        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string> { { "movieId", "DieHard" } }
        };

        var function = new GetMovieDirectorsFunction(_serviceProvider);
        var response = await function.HandleAsync(request, _context);

        await Verify(response);
    }

    [Fact]
    [TestPriority(8)]
    public async Task GetCharacterMoviesTest()
    {
        DynamoDbSetup(seed: true);

        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string> { { "characterId", "JohnMcClane" } }
        };

        var function = new GetCharacterMoviesFunction(_serviceProvider);
        var response = await function.HandleAsync(request, _context);

        await Verify(response);
    }

    [Fact]
    [TestPriority(9)]
    public async Task GetDirectorMoviesTest()
    {
        DynamoDbSetup(seed: true);

        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string> { { "directorId", "RennyHarlin" } }
        };

        var function = new GetDirectorMoviesFunction(_serviceProvider);
        var response = await function.HandleAsync(request, _context);

        await Verify(response);
    }

    public Task InitializeAsync()
    {
        return _dynamoDbLocal.StartAsync();
    }

    public Task DisposeAsync()
    {
        return this._dynamoDbLocal.DisposeAsync().AsTask();
    }

    private void DynamoDbSetup(bool seed = true)
    {
        var cd = Directory.GetCurrentDirectory();
        var wd = Path.Combine(cd.Substring(0, cd.IndexOf("test/MovieApi.Tests")), "src/MovieApi");
        var b = new Bash(wd);

        b.Run("npm run migrate", s => _output.WriteLine(s));

        if (seed)
        {
            b.Run("npm run seed:local", s => _output.WriteLine(s));
        }
    }
}
