using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using FluentValidation;
using Mediator;
using MovieApi.Domain;
using MovieApi.Requests;
using MovieApi.Responses;

namespace MovieApi.Handlers;

public class GetMoviesRequestHandler : IRequestHandler<GetMoviesRequest, Response<List<Movie>>>
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly IValidator<GetMoviesRequest> _validator;

    public GetMoviesRequestHandler(IAmazonDynamoDB dynamoDbClient, IValidator<GetMoviesRequest> validator)
    {
        _dynamoDbClient = dynamoDbClient;
        _validator = validator;
    }

    public async ValueTask<Response<List<Movie>>> Handle(GetMoviesRequest request, CancellationToken cancellationToken)
    {
        var result = _validator.Validate(request);

        if (!result.IsValid)
        {
            return new Response<List<Movie>>(400, result.ToString());
        }

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

        var tableName = Environment.GetEnvironmentVariable("TABLE_NAME") ?? "MoviesTable-dev";
        var table = Table.LoadTable(_dynamoDbClient, new TableConfig(tableName));
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