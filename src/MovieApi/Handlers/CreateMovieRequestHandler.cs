using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using FluentValidation;
using Mediator;
using MovieApi.Domain;
using MovieApi.Requests;
using MovieApi.Responses;

namespace MovieApi.Handlers;

public class CreateMovieRequestHandler : IRequestHandler<CreateMovieRequest, Response<Movie>>
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly IValidator<CreateMovieRequest> _validator;

    public CreateMovieRequestHandler(IAmazonDynamoDB dynamoDbClient, IValidator<CreateMovieRequest> validator)
    {
        _dynamoDbClient = dynamoDbClient;
        _validator = validator;
    }

    public async ValueTask<Response<Movie>> Handle(CreateMovieRequest request, CancellationToken cancellationToken)
    {
        var result = _validator.Validate(request);

        if (!result.IsValid)
        {
            return new Response<Movie>(400, result.ToString());
        }

        var movie = request.Movie;
        var d = new Document();
        d["type"] = "movie";
        d["pk"] = $"MOVIE#{movie.MovieId}";
        d["sk"] = $"MOVIE#{movie.MovieId}";
        d["movieId"] = movie.MovieId;
        d["title"] = movie.Title;
        d["year"] = movie.Year;
        d["category"] = movie.Category;
        d["budget"] = movie.Budget;
        d["boxOffice"] = movie.BoxOffice;
        d["gsi2pk"] = movie.Category;
        d["gsi2sk"] = movie.Year.ToString();

        var table = Table.LoadTable(_dynamoDbClient, new TableConfig("MoviesTable"));
        await table.PutItemAsync(d);

        return new Response<Movie>(movie);
    }
}