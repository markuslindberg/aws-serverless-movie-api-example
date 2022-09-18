using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MovieApi.Domain;
using MovieApi.Requests;

namespace MovieApi.Functions;

public sealed class UpdateMovieFunction : RequestResponseFunctionBase
{
    public UpdateMovieFunction() : this(Startup.Configure().BuildServiceProvider())
    {
    }

    public UpdateMovieFunction(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override async Task<APIGatewayProxyResponse> HandleRequest(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var movieId = request.PathParameters["movieId"];
        var movie = JsonSerializer.Deserialize<Movie>(request.Body, JsonSerializerOptions)!;
        var mediator = ServiceProvider.GetRequiredService<IMediator>();
        var response = await mediator.Send(new UpdateMovieRequest(movieId, movie));

        return new APIGatewayProxyResponse
        {
            StatusCode = response.StatusCode,
            Body = response.StatusCode == 200
                ? JsonSerializer.Serialize(response.Result, JsonSerializerOptions)
                : response.ErrorMessage
        };
    }
}