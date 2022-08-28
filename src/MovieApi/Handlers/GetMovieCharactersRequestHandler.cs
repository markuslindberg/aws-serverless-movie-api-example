using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using MediatR;
using MovieApi.Domain;
using MovieApi.Requests;
using MovieApi.Responses;

namespace MovieApi.Handlers;

public class GetMovieCharactersRequestHandler : IRequestHandler<GetMovieCharactersRequest, Response<List<Character>>>
{
    private readonly IAmazonDynamoDB _dynamoDbClient;

    public GetMovieCharactersRequestHandler(IAmazonDynamoDB dynamoDbClient)
    {
        _dynamoDbClient = dynamoDbClient;
    }

    public async Task<Response<List<Character>>> Handle(GetMovieCharactersRequest request, CancellationToken cancellationToken)
    {
        var table = Table.LoadTable(_dynamoDbClient, new TableConfig("MoviesTable"));
        var filter = new QueryFilter("sk", QueryOperator.BeginsWith, "CHARACTER#");
        var search = table.Query($"MOVIE#{request.MovieId}", filter);
        var docs = await search.GetNextSetAsync();

        return new Response<List<Character>>(docs.Select(d => new Character(
            d["characterId"].AsString(),
            d["name"].AsString(),
            d["playedBy"].AsString(),
            d["role"].AsString(),
            d["nationality"].AsString()))
        .ToList());
    }
}