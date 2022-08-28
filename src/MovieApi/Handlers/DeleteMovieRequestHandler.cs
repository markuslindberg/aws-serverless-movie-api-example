using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using MediatR;
using MovieApi.Domain;
using MovieApi.Requests;
using MovieApi.Responses;

namespace MovieApi.Handlers;

public class DeleteMovieRequestHandler : IRequestHandler<DeleteMovieRequest, Response<Movie>>
{
    private readonly IAmazonDynamoDB _dynamoDbClient;

    public DeleteMovieRequestHandler(IAmazonDynamoDB dynamoDbClient)
    {
        _dynamoDbClient = dynamoDbClient;
    }

    public async Task<Response<Movie>> Handle(DeleteMovieRequest request, CancellationToken cancellationToken)
    {
        var key = $"MOVIE#{request.MovieId}";
        var table = Table.LoadTable(_dynamoDbClient, new TableConfig("MoviesTable"));
        var d = await table.DeleteItemAsync(key, key, new DeleteItemOperationConfig
        {
            ReturnValues = ReturnValues.AllOldAttributes
        });

        if (d == null || d.Count == 0)
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