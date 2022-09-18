using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MovieApi.Requests;

namespace MovieApi.Functions;

public sealed class GetMovieDirectorsFunction : RequestResponseFunctionBase
{
    public GetMovieDirectorsFunction() : this(Startup.Configure().BuildServiceProvider())
    {
    }

    public GetMovieDirectorsFunction(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override async Task<APIGatewayProxyResponse> HandleRequest(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var movieId = request.PathParameters["movieId"];
        var mediator = ServiceProvider.GetRequiredService<IMediator>();
        var response = await mediator.Send(new GetMovieDirectorsRequest(movieId));

        return new APIGatewayProxyResponse
        {
            StatusCode = response.StatusCode,
            Body = response.StatusCode == 200
                ? JsonSerializer.Serialize(response.Result, JsonSerializerOptions)
                : response.ErrorMessage
        };
    }
}