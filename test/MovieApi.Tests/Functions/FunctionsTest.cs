using System.Text;
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
        var response = await _clientFactory
            .CreateClient("aws-client").GetAsync("movies/DieHard");

        var content = await response.Content.ReadAsStringAsync();

        await Verify(content);
    }

    [Theory]
    [InlineData(200, "DieHard")]
    [InlineData(400, "DieHard_$£!")]
    [InlineData(404, "DieHard999")]
    public async Task GetMovieStatusCodeTheory(int expectedStatusCode, string movieId)
    {
        var response = await _clientFactory.CreateClient("aws-client")
            .GetAsync($"movies/{movieId}");

        Assert.Equal(expectedStatusCode, (int)response.StatusCode);
    }

    [Fact]
    public async Task GetMoviesShouldReturnList()
    {
        var response = await _clientFactory.CreateClient("aws-client")
            .GetAsync("movies?category=Action&year-min=1989");

        var content = await response.Content.ReadAsStringAsync();

        Assert.Matches("^\\[.*\\]$", content);
    }

    [Theory]
    [InlineData(200, "Action")]
    [InlineData(200, "Drama")]
    [InlineData(400, "")]
    [InlineData(400, null)]
    public async Task GetMoviesStatusCodeTheory(int expectedStatusCode, string category)
    {
        var response = await _clientFactory.CreateClient("aws-client")
            .GetAsync($"movies?category={category}");

        Assert.Equal(expectedStatusCode, (int)response.StatusCode);
    }

    [Fact]
    public async Task GetMovieCharactersShouldMatchExpectedResponse()
    {
        var response = await _clientFactory.CreateClient("aws-client")
            .GetAsync("movies/DieHard/characters");

        var content = await response.Content.ReadAsStringAsync();

        await Verify(content);
    }

    [Theory]
    [InlineData(200, "DieHard")]
    [InlineData(400, "DieHard_$£!")]
    public async Task GetMovieCharactersStatusCodeTheory(int expectedStatusCode, string movieId)
    {
        var response = await _clientFactory.CreateClient("aws-client")
            .GetAsync($"movies/{movieId}/characters");

        Assert.Equal(expectedStatusCode, (int)response.StatusCode);
    }

    [Fact]
    public async Task GetMovieDirectorsShouldMatchExpectedResponse()
    {
        var response = await _clientFactory.CreateClient("aws-client")
            .GetAsync("movies/DieHard/directors");

        var content = await response.Content.ReadAsStringAsync();

        await Verify(content);
    }

    [Theory]
    [InlineData(200, "DieHard")]
    [InlineData(400, "DieHard_$£!")]
    public async Task GetMovieDirectorsStatusCodeTheory(int expectedStatusCode, string movieId)
    {
        var response = await _clientFactory.CreateClient("aws-client")
            .GetAsync($"movies/{movieId}/directors");

        Assert.Equal(expectedStatusCode, (int)response.StatusCode);
    }

    [Fact]
    public async Task GetCharacterMoviesShouldMatchExpectedResponse()
    {
        var response = await _clientFactory.CreateClient("aws-client")
            .GetAsync("characters/JohnMcClane/movies");

        var content = await response.Content.ReadAsStringAsync();

        await Verify(content);
    }

    [Theory]
    [InlineData(200, "JohnMcClane")]
    [InlineData(400, "JohnMcClane_$£!")]
    public async Task GetCharacterMoviesStatusCodeTheory(int expectedStatusCode, string characterId)
    {
        var response = await _clientFactory.CreateClient("aws-client")
            .GetAsync($"characters/{characterId}/movies");

        Assert.Equal(expectedStatusCode, (int)response.StatusCode);
    }

    [Fact]
    public async Task GetDirectorMoviesShouldMatchExpectedResponse()
    {
        var response = await _clientFactory.CreateClient("aws-client")
            .GetAsync("directors/RennyHarlin/movies");

        var content = await response.Content.ReadAsStringAsync();

        await Verify(content);
    }

    [Theory]
    [InlineData(200, "RennyHarlin")]
    [InlineData(400, "RennyHarlin_$£!")]
    public async Task GetDirectorMoviesStatusCodeTheory(int expectedStatusCode, string directorId)
    {
        var response = await _clientFactory.CreateClient("aws-client")
            .GetAsync($"directors/{directorId}/movies");

        Assert.Equal(expectedStatusCode, (int)response.StatusCode);
    }

    [Fact]
    public async Task CreateMovieShouldMatchExpectedResponse()
    {
        var requestBody = new StringContent("{\"movieId\": \"DieHard7\", \"title\": \"Die Hard 7\", \"year\": 2035, \"category\": \"Action\", \"budget\": \"Unlimited\", \"boxOffice\": \"N/A\"}",
            Encoding.UTF8, "application/json");

        var response = await _clientFactory.CreateClient("aws-client")
            .PostAsync("movies", requestBody);

        var content = await response.Content.ReadAsStringAsync();

        await Verify(content);
    }

    [Theory]
    [InlineData(400, "{\"movieId\": \"DieHard_$£!\"}")]
    [InlineData(400, "{\"invalid_json\"")]
    public async Task CreateMovieStatusCodeTheory(int expectedStatusCode, string body)
    {
        var requestBody = new StringContent(body,
            Encoding.UTF8, "application/json");

        var response = await _clientFactory.CreateClient("aws-client")
            .PostAsync("movies", requestBody);

        Assert.Equal(expectedStatusCode, (int)response.StatusCode);
    }

    [Fact]
    public async Task UpdateMovieShouldMatchExpectedResponse()
    {
        var requestBody = new StringContent("{\"movieId\": \"DieHard8\", \"title\": \"Die Hard 8\", \"year\": 2035, \"category\": \"Action\", \"budget\": \"Unlimited\", \"boxOffice\": \"N/A\"}",
            Encoding.UTF8, "application/json");

        var response = await _clientFactory.CreateClient("aws-client")
            .PutAsync("movies/DieHard8", requestBody);

        var content = await response.Content.ReadAsStringAsync();

        await Verify(content);
    }

    [Theory]
    [InlineData(400, "DieHard1", "{\"movieId\": \"DieHard2\"}")]
    [InlineData(400, "DieHard_$£!", "{\"movieId\": \"DieHard_$£!\"}")]
    [InlineData(400, "DieHard", "{\"invalid_json\"")]
    public async Task UpdateMovieStatusCodeTheory(int expectedStatusCode, string movieId, string body)
    {
        var requestBody = new StringContent(body,
            Encoding.UTF8, "application/json");

        var response = await _clientFactory.CreateClient("aws-client")
            .PutAsync($"movies/{movieId}", requestBody);

        Assert.Equal(expectedStatusCode, (int)response.StatusCode);
    }

    [Fact]
    public async Task DeleteMovieShouldMatchExpectedResponse()
    {
        await _clientFactory.CreateClient("aws-client")
            .PostAsync("movies", new StringContent("{\"movieId\": \"DieHard9\", \"title\": \"Die Hard 9\", \"year\": 2035, \"category\": \"Action\", \"budget\": \"Unlimited\", \"boxOffice\": \"N/A\"}",
                Encoding.UTF8, "application/json"));

        var response = await _clientFactory.CreateClient("aws-client")
            .DeleteAsync("movies/DieHard9");

        var content = await response.Content.ReadAsStringAsync();

        await Verify(content);
    }

    [Theory]
    [InlineData(404, "DieHard999")]
    [InlineData(400, "DieHard_$£!")]
    public async Task DeleteMovieStatusCodeTheory(int expectedStatusCode, string movieId)
    {
        var response = await _clientFactory.CreateClient("aws-client")
            .DeleteAsync($"movies/{movieId}");

        Assert.Equal(expectedStatusCode, (int)response.StatusCode);
    }
}