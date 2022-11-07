using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using MovieApi.Requests;

namespace MovieApi.Functions;

public sealed class DeleteMovieFunction : RequestResponseFunctionBase
{
    public DeleteMovieFunction() : this(Startup.Configure().BuildServiceProvider())
    {
    }

    public DeleteMovieFunction(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override async Task<APIGatewayProxyResponse> HandleRequest(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var movieId = request.PathParameters["movieId"];
        var mediator = ServiceProvider.GetRequiredService<IMediator>();
        var response = await mediator.Send(new DeleteMovieRequest(movieId));

        return ToAPIGatewayProxyResponse(response);
    }
}