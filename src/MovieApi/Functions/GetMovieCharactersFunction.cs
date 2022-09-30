using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MovieApi.Requests;

namespace MovieApi.Functions;

public sealed class GetMovieCharactersFunction : RequestResponseFunctionBase
{
    public GetMovieCharactersFunction() : this(Startup.Configure().BuildServiceProvider())
    {
    }

    public GetMovieCharactersFunction(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override async Task<APIGatewayProxyResponse> HandleRequest(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var movieId = request.PathParameters["movieId"];
        var mediator = ServiceProvider.GetRequiredService<IMediator>();
        var response = await mediator.Send(new GetMovieCharactersRequest(movieId));

        return ToAPIGatewayProxyResponse(response);
    }
}