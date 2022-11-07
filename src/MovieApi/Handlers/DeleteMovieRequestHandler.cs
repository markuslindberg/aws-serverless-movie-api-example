using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using FluentValidation;
using Mediator;
using MovieApi.Domain;
using MovieApi.Requests;
using MovieApi.Responses;

namespace MovieApi.Handlers;

public class DeleteMovieRequestHandler : IRequestHandler<DeleteMovieRequest, Response<Movie>>
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly IValidator<DeleteMovieRequest> _validator;

    public DeleteMovieRequestHandler(IAmazonDynamoDB dynamoDbClient, IValidator<DeleteMovieRequest> validator)
    {
        _dynamoDbClient = dynamoDbClient;
        _validator = validator;
    }

    public async ValueTask<Response<Movie>> Handle(DeleteMovieRequest request, CancellationToken cancellationToken)
    {
        var result = _validator.Validate(request);

        if (!result.IsValid)
        {
            return new Response<Movie>(400, result.ToString());
        }

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