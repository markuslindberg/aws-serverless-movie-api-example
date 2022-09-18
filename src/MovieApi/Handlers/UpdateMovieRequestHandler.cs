using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using FluentValidation;
using MediatR;
using MovieApi.Domain;
using MovieApi.Requests;
using MovieApi.Responses;

namespace MovieApi.Handlers;

public class UpdateMovieRequestHandler : IRequestHandler<UpdateMovieRequest, Response<Movie>>
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly IValidator<UpdateMovieRequest> _validator;

    public UpdateMovieRequestHandler(IAmazonDynamoDB dynamoDbClient, IValidator<UpdateMovieRequest> validator)
    {
        _dynamoDbClient = dynamoDbClient;
        _validator = validator;
    }

    public async Task<Response<Movie>> Handle(UpdateMovieRequest request, CancellationToken cancellationToken)
    {
        var result = _validator.Validate(request);

        if (!result.IsValid)
        {
            return new Response<Movie>(400, result.ToString());
        }

        var movie = request.Movie;
        var d = new Document();
        d["type"] = "movie";
        d["pk"] = $"MOVIE#{request.MovieId}";
        d["sk"] = $"MOVIE#{request.MovieId}";
        d["movieId"] = request.MovieId;
        d["title"] = movie.Title;
        d["year"] = movie.Year;
        d["category"] = movie.Category;
        d["budget"] = movie.Budget;
        d["boxOffice"] = movie.BoxOffice;
        d["gsi2pk"] = movie.Category;
        d["gsi2sk"] = movie.Year.ToString();

        var table = Table.LoadTable(_dynamoDbClient, new TableConfig("MoviesTable"));
        await table.UpdateItemAsync(d, new UpdateItemOperationConfig
        {
            ReturnValues = ReturnValues.AllNewAttributes
        });

        return new Response<Movie>(movie);
    }
}