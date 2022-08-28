using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using MediatR;
using MovieApi.Domain;
using MovieApi.Requests;
using MovieApi.Responses;

namespace MovieApi.Handlers;

public class UpdateMovieRequestHandler : IRequestHandler<UpdateMovieRequest, Response<Movie>>
{
    private readonly IAmazonDynamoDB _dynamoDbClient;

    public UpdateMovieRequestHandler(IAmazonDynamoDB dynamoDbClient)
    {
        _dynamoDbClient = dynamoDbClient;
    }

    public async Task<Response<Movie>> Handle(UpdateMovieRequest request, CancellationToken cancellationToken)
    {
        var movie = request.Movie;

        var d = new Document();
        d["type"] = "movie";
        d["pk"] = $"MOVIE#{request.MovieId}";
        d["sk"] = $"MOVIE#{request.MovieId}";
        d["movieId"] = request.MovieId;
        d["title"] = movie.Title;
        d["year"] = movie.Year;
        d["category"] = movie.Category;
        d["budget"] = movie.Budget;
        d["boxOffice"] = movie.BoxOffice;
        d["gsi2pk"] = movie.Category;
        d["gsi2sk"] = movie.Year.ToString();

        var table = Table.LoadTable(_dynamoDbClient, new TableConfig("MoviesTable"));
        await table.UpdateItemAsync(d, new UpdateItemOperationConfig
        {
            ReturnValues = ReturnValues.AllNewAttributes
        });

        return new Response<Movie>(movie);
    }
}