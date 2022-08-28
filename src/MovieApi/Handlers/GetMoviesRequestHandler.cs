using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using MediatR;
using MovieApi.Domain;
using MovieApi.Requests;
using MovieApi.Responses;

namespace MovieApi.Handlers;

public class GetMoviesRequestHandler : IRequestHandler<GetMoviesRequest, Response<List<Movie>>>
{
    private readonly IAmazonDynamoDB _dynamoDbClient;

    public GetMoviesRequestHandler(IAmazonDynamoDB dynamoDbClient)
    {
        _dynamoDbClient = dynamoDbClient;
    }

    public async Task<Response<List<Movie>>> Handle(GetMoviesRequest request, CancellationToken cancellationToken)
    {
        var filter = new QueryFilter("gsi2pk", QueryOperator.Equal, request.Category);
        if (request.YearMin != null)
            filter.AddCondition("gsi2sk", QueryOperator.GreaterThanOrEqual, request.YearMin.ToString());
        if (request.YearMax != null)
            filter.AddCondition("gsi2sk", QueryOperator.LessThanOrEqual, request.YearMax.ToString());

        var query = new QueryOperationConfig
        {
            IndexName = "gsi2",
            Filter = filter
        };

        var table = Table.LoadTable(_dynamoDbClient, new TableConfig("MoviesTable"));
        var search = table.Query(query);
        var docs = await search.GetNextSetAsync();

        return new Response<List<Movie>>(docs.Select(d => new Movie(
            d["movieId"].AsString(),
            d["title"].AsString(),
            d["year"].AsInt(),
            d["category"].AsString(),
            d["budget"].AsString(),
            d["boxOffice"].AsString()))
        .ToList());
    }
}