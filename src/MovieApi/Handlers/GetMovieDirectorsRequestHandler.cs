using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using FluentValidation;
using Mediator;
using MovieApi.Domain;
using MovieApi.Requests;
using MovieApi.Responses;

namespace MovieApi.Handlers;

public class GetMovieDirectorsRequestHandler : IRequestHandler<GetMovieDirectorsRequest, Response<List<Director>>>
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly IValidator<GetMovieDirectorsRequest> _validator;

    public GetMovieDirectorsRequestHandler(IAmazonDynamoDB dynamoDbClient, IValidator<GetMovieDirectorsRequest> validator)
    {
        _dynamoDbClient = dynamoDbClient;
        _validator = validator;
    }

    public async ValueTask<Response<List<Director>>> Handle(GetMovieDirectorsRequest request, CancellationToken cancellationToken)
    {
        var result = _validator.Validate(request);

        if (!result.IsValid)
        {
            return new Response<List<Director>>(400, result.ToString());
        }

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