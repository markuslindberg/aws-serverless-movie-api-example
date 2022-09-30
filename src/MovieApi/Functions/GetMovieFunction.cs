using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MovieApi.Requests;

namespace MovieApi.Functions;

public sealed class GetMovieFunction : RequestResponseFunctionBase
{
    public GetMovieFunction() : this(Startup.Configure().BuildServiceProvider())
    {
    }

    public GetMovieFunction(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override async Task<APIGatewayProxyResponse> HandleRequest(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var movieId = request.PathParameters["movieId"];
        var mediator = ServiceProvider.GetRequiredService<IMediator>();
        var response = await mediator.Send(new GetMovieRequest(movieId));

        return ToAPIGatewayProxyResponse(response);
    }
}