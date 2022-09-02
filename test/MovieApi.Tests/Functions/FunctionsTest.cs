using System.Reflection;
using Amazon.DynamoDBv2;
using Amazon.Lambda.TestUtilities;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MovieApi.Domain;
using MovieApi.Requests;
using MovieApi.Tests.Infrastructure;
using Serilog;
using Xunit.Abstractions;

namespace MovieApi.Tests.Functions;

[UsesVerify]
[TestCaseOrderer("MovieApi.Tests.Infrastructure.PriorityOrderer", "MovieApi.Tests")]
public class FunctionsTest : IAsyncLifetime
{
    private readonly IServiceProvider _serviceProvider;
    private readonly MovieApi.Functions _functions;
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
        serviceContainer.AddSingleton<IAmazonDynamoDB>(_dynamoDbLocalClient);
        serviceContainer.AddSingleton<ILogger>(new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.TestOutput(output, Serilog.Events.LogEventLevel.Verbose)
            .CreateLogger());
        _serviceProvider = serviceContainer.BuildServiceProvider();
        _functions = new MovieApi.Functions(_serviceProvider);
        _context = new TestLambdaContext();
        _output = output;
    }

    [Fact]
    [TestPriority(1)]
    public async Task GetMovieTest()
    {
        DynamoDbSetup(seed: true);

        var request = new GetMovieRequest("DieHard");
        var response = await _functions.GetMovie(request, _context);

        await Verify(response);
    }

    [Fact]
    [TestPriority(2)]
    public async Task GetMoviesTest()
    {
        DynamoDbSetup(seed: true);

        var request = new GetMoviesRequest("Action");
        var response = await _functions.GetMovies(request, _context);

        await Verify(response);
    }

    [Fact]
    [TestPriority(3)]
    public async Task CreateMovieTest()
    {
        DynamoDbSetup(seed: false);

        var request = new CreateMovieRequest(new Movie("DieHard3", "Die Hard 3", 2022, "Action", "Unlimited", "N/A"));
        var createrResponse = await _functions.CreateMovie(request, _context);
        var getResponse = await _functions.GetMovie(new GetMovieRequest("DieHard3"), _context);

        await Verify(new
        {
            createrResponse,
            getResponse
        });
    }

    [Fact]
    [TestPriority(4)]
    public async Task UpdateMovieTest()
    {
        DynamoDbSetup(seed: true);

        var request = new UpdateMovieRequest("DieHard", new Movie("DieHard", "Die Hard", 2022, "Action", "Unlimited", "N/A"));
        var updateResponse = await _functions.UpdateMovie(request, _context);
        var getResponse = await _functions.GetMovie(new GetMovieRequest("DieHard"), _context);

        await Verify(new
        {
            updateResponse,
            getResponse
        });
    }

    [Fact]
    [TestPriority(5)]
    public async Task DeleteMovieTest()
    {
        DynamoDbSetup(seed: true);

        var getResponse = await _functions.GetMovie(new GetMovieRequest("DieHard"), _context);
        var deleteResponse = await _functions.DeleteMovie(new DeleteMovieRequest("DieHard"), _context);
        var deleteAgainResponse = await _functions.DeleteMovie(new DeleteMovieRequest("DieHard"), _context);

        await Verify(new
        {
            getResponse,
            deleteResponse,
            deleteAgainResponse
        });
    }

    [Fact]
    [TestPriority(6)]
    public async Task GetMovieCharactersTest()
    {
        DynamoDbSetup(seed: true);

        var request = new GetMovieCharactersRequest("DieHard");
        var response = await _functions.GetMovieCharacters(request, _context);

        await Verify(response);
    }

    [Fact]
    [TestPriority(7)]
    public async Task GetMovieDirectorsTest()
    {
        DynamoDbSetup(seed: true);

        var request = new GetMovieDirectorsRequest("DieHard");
        var response = await _functions.GetMovieDirectors(request, _context);

        await Verify(response);
    }

    [Fact]
    [TestPriority(8)]
    public async Task GetCharacterMoviesTest()
    {
        DynamoDbSetup(seed: true);

        var request = new GetCharacterMoviesRequest("JohnMcClane");
        var response = await _functions.GetCharacterMovies(request, _context);

        await Verify(response);
    }

    [Fact]
    [TestPriority(9)]
    public async Task GetDirectorMoviesTest()
    {
        DynamoDbSetup(seed: true);

        var request = new GetDirectorMoviesRequest("RennyHarlin");
        var response = await _functions.GetDirectorMovies(request, _context);

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
