using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MovieApi.Requests;

namespace MovieApi.Functions;

public sealed class GetCharacterMoviesFunction : RequestResponseFunctionBase
{
    public GetCharacterMoviesFunction() : this(Startup.Configure().BuildServiceProvider())
    {
    }

    public GetCharacterMoviesFunction(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override async Task<APIGatewayProxyResponse> HandleRequest(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var characterId = request.PathParameters["characterId"];
        var mediator = ServiceProvider.GetRequiredService<IMediator>();
        var response = await mediator.Send(new GetCharacterMoviesRequest(characterId));

        return new APIGatewayProxyResponse
        {
            StatusCode = response.StatusCode,
            Body = response.StatusCode == 200
                ? JsonSerializer.Serialize(response.Result, JsonSerializerOptions)
                : response.ErrorMessage
        };
    }
}