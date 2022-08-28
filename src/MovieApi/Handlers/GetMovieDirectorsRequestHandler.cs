using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using MediatR;
using MovieApi.Domain;
using MovieApi.Requests;
using MovieApi.Responses;

namespace MovieApi.Handlers;

public class GetMovieDirectorsRequestHandler : IRequestHandler<GetMovieDirectorsRequest, Response<List<Director>>>
{
    private readonly IAmazonDynamoDB _dynamoDbClient;

    public GetMovieDirectorsRequestHandler(IAmazonDynamoDB dynamoDbClient)
    {
        _dynamoDbClient = dynamoDbClient;
    }

    public async Task<Response<List<Director>>> Handle(GetMovieDirectorsRequest request, CancellationToken cancellationToken)
    {
        var table = Table.LoadTable(_dynamoDbClient, new TableConfig("MoviesTable"));
        var filter = new QueryFilter("sk", QueryOperator.BeginsWith, "DIRECTOR#");
        var search = table.Query($"MOVIE#{request.MovieId}", filter);
        var docs = await search.GetNextSetAsync();

        return new Response<List<Director>>(docs.Select(d => new Director(
            d["directorId"].AsString(),
            d["name"].AsString()))
        .ToList());
    }
}