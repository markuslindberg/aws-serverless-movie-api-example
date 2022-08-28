using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using MediatR;
using MovieApi.Domain;
using MovieApi.Requests;
using MovieApi.Responses;

namespace MovieApi.Handlers;

public class CreateMovieRequestHandler : IRequestHandler<CreateMovieRequest, Response<Movie>>
{
    private readonly IAmazonDynamoDB _dynamoDbClient;

    public CreateMovieRequestHandler(IAmazonDynamoDB dynamoDbClient)
    {
        _dynamoDbClient = dynamoDbClient;
    }

    public async Task<Response<Movie>> Handle(CreateMovieRequest request, CancellationToken cancellationToken)
    {
        var movie = request.Movie;

        var d = new Document();
        d["type"] = "movie";
        d["pk"] = $"MOVIE#{movie.MovieId}";
        d["sk"] = $"MOVIE#{movie.MovieId}";
        d["movieId"] = movie.MovieId;
        d["title"] = movie.Title;
        d["year"] = movie.Year;
        d["category"] = movie.Category;
        d["budget"] = movie.Budget;
        d["boxOffice"] = movie.BoxOffice;
        d["gsi2pk"] = movie.Category;
        d["gsi2sk"] = movie.Year.ToString();

        var table = Table.LoadTable(_dynamoDbClient, new TableConfig("MoviesTable"));
        await table.PutItemAsync(d);

        return new Response<Movie>(movie);
    }
}