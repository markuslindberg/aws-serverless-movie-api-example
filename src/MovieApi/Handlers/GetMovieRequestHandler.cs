using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using MediatR;
using MovieApi.Domain;
using MovieApi.Requests;
using MovieApi.Responses;

namespace MovieApi.Handlers;

public class GetMovieRequestHandler : IRequestHandler<GetMovieRequest, Response<Movie>>
{
    private readonly IAmazonDynamoDB _dynamoDbClient;

    public GetMovieRequestHandler(IAmazonDynamoDB dynamoDbClient)
    {
        _dynamoDbClient = dynamoDbClient;
    }

    public async Task<Response<Movie>> Handle(GetMovieRequest request, CancellationToken cancellationToken)
    {
        var key = $"MOVIE#{request.MovieId}";
        var table = Table.LoadTable(_dynamoDbClient, new TableConfig("MoviesTable"));
        var d = await table.GetItemAsync(key, key);

        if (d == null)
        {
            return new Response<Movie>(404, $"Movie \"{request.MovieId}\" not found");
        }

        return new Response<Movie>(new Movie(
            d["movieId"].AsString(),
            d["title"].AsString(),
            d["year"].AsInt(),
            d["category"].AsString(),
            d["budget"].AsString(),
            d["boxOffice"].AsString()));
    }
}