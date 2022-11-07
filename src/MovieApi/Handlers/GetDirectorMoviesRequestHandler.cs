using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using FluentValidation;
using Mediator;
using MovieApi.Requests;
using MovieApi.Responses;

namespace MovieApi.Handlers;

public class GetDirectorMoviesRequestHandler : IRequestHandler<GetDirectorMoviesRequest, Response<List<string>>>
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly IValidator<GetDirectorMoviesRequest> _validator;

    public GetDirectorMoviesRequestHandler(IAmazonDynamoDB dynamoDbClient, IValidator<GetDirectorMoviesRequest> validator)
    {
        _dynamoDbClient = dynamoDbClient;
        _validator = validator;
    }

    public async ValueTask<Response<List<string>>> Handle(GetDirectorMoviesRequest request, CancellationToken cancellationToken)
    {
        var result = _validator.Validate(request);

        if (!result.IsValid)
        {
            return new Response<List<string>>(400, result.ToString());
        }

        var filter = new QueryFilter("gsi1pk", QueryOperator.Equal, $"DIRECTOR#{request.DirectorId}");
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