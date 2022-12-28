using Amazon.Lambda.APIGatewayEvents;
using MovieApi.Functions;
using Xunit.Abstractions;

namespace MovieApi.Tests.Functions;

[UsesVerify]
public class FunctionsTest : FunctionsTestBase
{
    public FunctionsTest(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task GetMovieShouldMatchExpectedResponse()
    {
        var request = CreateRequestWithPathParams("movieId", "DieHard");

        var function = new GetMovieFunction(_serviceProvider);
        var response = await function.HandleAsync(request, _context);

        await Verify(response);
    }

    [Theory]
    [InlineData(200, "DieHard")]
    [InlineData(400, "DieHard@#$£!")]
    [InlineData(404, "DieHard999")]
    public async Task GetMovieStatusCodeTheory(int expectedStatusCode, string movieId)
    {
        var request = CreateRequestWithPathParams("movieId", movieId);

        var function = new GetMovieFunction(_serviceProvider);
        var response = await function.HandleAsync(request, _context);

        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Fact]
    public async Task GetMoviesShouldReturnList()
    {
        var request = CreateRequestWithQueryParams(new Dictionary<string, string>
        {
            { "category", "Action" },
            { "year-min", "1989" }
        });

        var function = new GetMoviesFunction(_serviceProvider);
        var response = await function.HandleAsync(request, _context);

        Assert.Matches("^\\[.*\\]$", response.Body);
    }

    [Theory]
    [InlineData(200, "Action")]
    [InlineData(200, "Drama")]
    [InlineData(400, "")]
    [InlineData(400, null)]
    public async Task GetMoviesStatusCodeTheory(int expectedStatusCode, string category)
    {
        var request = CreateRequestWithQueryParams("category", category);

        var function = new GetMoviesFunction(_serviceProvider);
        var response = await function.HandleAsync(request, _context);

        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Fact]
    public async Task GetMovieCharactersShouldMatchExpectedResponse()
    {
        var request = CreateRequestWithPathParams("movieId", "DieHard");

        var function = new GetMovieCharactersFunction(_serviceProvider);
        var response = await function.HandleAsync(request, _context);

        await Verify(response);
    }

    [Theory]
    [InlineData(200, "DieHard")]
    [InlineData(400, "DieHard@#$£!")]
    public async Task GetMovieCharactersStatusCodeTheory(int expectedStatusCode, string movieId)
    {
        var request = CreateRequestWithPathParams("movieId", movieId);

        var function = new GetMovieCharactersFunction(_serviceProvider);
        var response = await function.HandleAsync(request, _context);

        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Fact]
    public async Task GetMovieDirectorsShouldMatchExpectedResponse()
    {
        var request = CreateRequestWithPathParams("movieId", "DieHard");

        var function = new GetMovieDirectorsFunction(_serviceProvider);
        var response = await function.HandleAsync(request, _context);

        await Verify(response);
    }

    [Theory]
    [InlineData(200, "DieHard")]
    [InlineData(400, "DieHard@#$£!")]
    public async Task GetMovieDirectorsStatusCodeTheory(int expectedStatusCode, string movieId)
    {
        var request = CreateRequestWithPathParams("movieId", movieId);

        var function = new GetMovieDirectorsFunction(_serviceProvider);
        var response = await function.HandleAsync(request, _context);

        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Fact]
    public async Task GetCharacterMoviesShouldMatchExpectedResponse()
    {
        var request = CreateRequestWithPathParams("characterId", "JohnMcClane");

        var function = new GetCharacterMoviesFunction(_serviceProvider);
        var response = await function.HandleAsync(request, _context);

        await Verify(response);
    }

    [Theory]
    [InlineData(200, "JohnMcClane")]
    [InlineData(400, "JohnMcClane@#$£!")]
    public async Task GetCharacterMoviesStatusCodeTheory(int expectedStatusCode, string characterId)
    {
        var request = CreateRequestWithPathParams("characterId", characterId);

        var function = new GetCharacterMoviesFunction(_serviceProvider);
        var response = await function.HandleAsync(request, _context);

        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Fact]
    public async Task GetDirectorMoviesShouldMatchExpectedResponse()
    {
        var request = CreateRequestWithPathParams("directorId", "RennyHarlin");

        var function = new GetDirectorMoviesFunction(_serviceProvider);
        var response = await function.HandleAsync(request, _context);

        await Verify(response);
    }

    [Theory]
    [InlineData(200, "RennyHarlin")]
    [InlineData(400, "RennyHarlin@#$£!")]
    public async Task GetDirectorMoviesStatusCodeTheory(int expectedStatusCode, string directorId)
    {
        var request = CreateRequestWithPathParams("directorId", directorId);

        var function = new GetDirectorMoviesFunction(_serviceProvider);
        var response = await function.HandleAsync(request, _context);

        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Fact]
    public async Task CreateMovieShouldMatchExpectedResponse()
    {
        var request = new APIGatewayProxyRequest
        {
            Body = "{\"movieId\": \"DieHard7\", \"title\": \"Die Hard 7\", \"year\": 2035, \"category\": \"Action\", \"budget\": \"Unlimited\", \"boxOffice\": \"N/A\"}"
        };

        var function = new CreateMovieFunction(_serviceProvider);
        var response = await function.HandleAsync(request, _context);

        await Verify(response);
    }

    [Theory]
    [InlineData(400, "{\"movieId\": \"DieHard@#$£!\"}")]
    [InlineData(500, "{\"invalid_json\"")]
    public async Task CreateMovieStatusCodeTheory(int expectedStatusCode, string body)
    {
        var request = new APIGatewayProxyRequest { Body = body };

        var function = new CreateMovieFunction(_serviceProvider);
        var response = await function.HandleAsync(request, _context);

        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Fact]
    public async Task UpdateMovieShouldMatchExpectedResponse()
    {
        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string> { { "movieId", "DieHard8" } },
            Body = "{\"movieId\": \"DieHard8\", \"title\": \"Die Hard 8\", \"year\": 2035, \"category\": \"Action\", \"budget\": \"Unlimited\", \"boxOffice\": \"N/A\"}"
        };

        var function = new UpdateMovieFunction(_serviceProvider);
        var response = await function.HandleAsync(request, _context);

        await Verify(response);
    }

    [Theory]
    [InlineData(400, "DieHard1", "{\"movieId\": \"DieHard2\"}")]
    [InlineData(400, "DieHard@#$£!", "{\"movieId\": \"DieHard@#$£!\"}")]
    [InlineData(500, "", "{\"invalid_json\"")]
    public async Task UpdateMovieStatusCodeTheory(int expectedStatusCode, string movieId, string body)
    {
        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string> { { "movieId", movieId } },
            Body = body
        };

        var function = new UpdateMovieFunction(_serviceProvider);
        var response = await function.HandleAsync(request, _context);

        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Fact]
    public async Task DeleteMovieShouldMatchExpectedResponse()
    {
        await new CreateMovieFunction(_serviceProvider).HandleAsync(
            new APIGatewayProxyRequest
            {
                Body = "{\"movieId\": \"DieHard9\", \"title\": \"Die Hard 9\", \"year\": 2035, \"category\": \"Action\", \"budget\": \"Unlimited\", \"boxOffice\": \"N/A\"}"
            }, _context);

        var request = CreateRequestWithPathParams("movieId", "DieHard9");

        var function = new DeleteMovieFunction(_serviceProvider);
        var response = await function.HandleAsync(request, _context);

        await Verify(response);
    }

    [Theory]
    [InlineData(404, "DieHard999")]
    [InlineData(400, "DieHard@#$£!")]
    public async Task DeleteMovieStatusCodeTheory(int expectedStatusCode, string movieId)
    {
        var request = CreateRequestWithPathParams("movieId", movieId);

        var function = new DeleteMovieFunction(_serviceProvider);
        var response = await function.HandleAsync(request, _context);

        Assert.Equal(expectedStatusCode, response.StatusCode);
    }
}