using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using FluentValidation;
using Mediator;
using MovieApi.Domain;
using MovieApi.Requests;
using MovieApi.Responses;

namespace MovieApi.Handlers;

public class GetMovieRequestHandler : IRequestHandler<GetMovieRequest, Response<Movie>>
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly IValidator<GetMovieRequest> _validator;

    public GetMovieRequestHandler(IAmazonDynamoDB dynamoDbClient, IValidator<GetMovieRequest> validator)
    {
        _dynamoDbClient = dynamoDbClient;
        _validator = validator;
    }

    public async ValueTask<Response<Movie>> Handle(GetMovieRequest request, CancellationToken cancellationToken)
    {
        var result = _validator.Validate(request);

        if (!result.IsValid)
        {
            return new Response<Movie>(400, result.ToString());
        }

        var key = $"MOVIE#{request.MovieId}";
        var tableName = Environment.GetEnvironmentVariable("TABLE_NAME") ?? "MoviesTable-dev";
        var table = Table.LoadTable(_dynamoDbClient, new TableConfig(tableName));
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