using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MovieApi.Requests;

namespace MovieApi.Functions;

public sealed class GetDirectorMoviesFunction : RequestResponseFunctionBase
{
    public GetDirectorMoviesFunction() : this(Startup.Configure().BuildServiceProvider())
    {
    }

    public GetDirectorMoviesFunction(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override async Task<APIGatewayProxyResponse> HandleRequest(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var directorId = request.PathParameters["directorId"];
        var mediator = ServiceProvider.GetRequiredService<IMediator>();
        var response = await mediator.Send(new GetDirectorMoviesRequest(directorId));

        return new APIGatewayProxyResponse
        {
            StatusCode = response.StatusCode,
            Body = response.StatusCode == 200
                ? JsonSerializer.Serialize(response.Result, JsonSerializerOptions)
                : response.ErrorMessage
        };
    }
}