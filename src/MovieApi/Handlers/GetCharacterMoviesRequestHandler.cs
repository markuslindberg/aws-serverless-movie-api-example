using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using MediatR;
using MovieApi.Requests;
using MovieApi.Responses;

namespace MovieApi.Handlers;

public class GetCharacterMoviesRequestHandler : IRequestHandler<GetCharacterMoviesRequest, Response<List<string>>>
{
    private readonly IAmazonDynamoDB _dynamoDbClient;

    public GetCharacterMoviesRequestHandler(IAmazonDynamoDB dynamoDbClient)
    {
        _dynamoDbClient = dynamoDbClient;
    }

    public async Task<Response<List<string>>> Handle(GetCharacterMoviesRequest request, CancellationToken cancellationToken)
    {
        var filter = new QueryFilter("gsi1pk", QueryOperator.Equal, $"CHARACTER#{request.CharacterId}");
        filter.AddCondition("gsi1sk", QueryOperator.BeginsWith, "MOVIE#");

        var query = new QueryOperationConfig
        {
            IndexName = "gsi1",
            Filter = filter
        };

        var table = Table.LoadTable(_dynamoDbClient, new TableConfig("MoviesTable"));
        var search = table.Query(query);
        var docs = await search.GetNextSetAsync();

        return new Response<List<string>>(docs.Select(d => d["movieId"].AsString()).ToList());
    }
}