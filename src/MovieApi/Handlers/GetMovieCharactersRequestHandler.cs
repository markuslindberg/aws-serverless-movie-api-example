using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using FluentValidation;
using Mediator;
using MovieApi.Domain;
using MovieApi.Requests;
using MovieApi.Responses;

namespace MovieApi.Handlers;

public class GetMovieCharactersRequestHandler : IRequestHandler<GetMovieCharactersRequest, Response<List<Character>>>
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly IValidator<GetMovieCharactersRequest> _validator;

    public GetMovieCharactersRequestHandler(IAmazonDynamoDB dynamoDbClient, IValidator<GetMovieCharactersRequest> validator)
    {
        _dynamoDbClient = dynamoDbClient;
        _validator = validator;
    }

    public async ValueTask<Response<List<Character>>> Handle(GetMovieCharactersRequest request, CancellationToken cancellationToken)
    {
        var result = _validator.Validate(request);

        if (!result.IsValid)
        {
            return new Response<List<Character>>(400, result.ToString());
        }

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