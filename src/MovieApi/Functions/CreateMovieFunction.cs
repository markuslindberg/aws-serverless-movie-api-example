using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using MovieApi.Domain;
using MovieApi.Requests;

namespace MovieApi.Functions;

public sealed class CreateMovieFunction : RequestResponseFunctionBase
{
    public CreateMovieFunction() : this(Startup.Configure().BuildServiceProvider())
    {
    }

    public CreateMovieFunction(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override async Task<APIGatewayProxyResponse> HandleRequest(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var movie = JsonSerializer.Deserialize<Movie>(request.Body, JsonSerializerOptions)!;
        var mediator = ServiceProvider.GetRequiredService<IMediator>();
        var response = await mediator.Send(new CreateMovieRequest(movie));

        return ToAPIGatewayProxyResponse(response);
    }
}