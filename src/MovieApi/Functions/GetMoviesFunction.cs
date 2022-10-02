using System.Diagnostics.CodeAnalysis;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MovieApi.Extensions;
using MovieApi.Requests;

namespace MovieApi.Functions;

public sealed class GetMoviesFunction : RequestResponseFunctionBase
{
    public GetMoviesFunction() : this(Startup.Configure().BuildServiceProvider())
    {
    }

    public GetMoviesFunction(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override async Task<APIGatewayProxyResponse> HandleRequest(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var category = request.QueryStringParameters.GetValueOrDefault("category", "")!;
        var yearMin = request.QueryStringParameters.GetValueOrDefault("year-min").ToInt();
        var yearMax = request.QueryStringParameters.GetValueOrDefault("year-max").ToInt();

        var mediator = ServiceProvider.GetRequiredService<IMediator>();
        var response = await mediator.Send(new GetMoviesRequest(category, yearMin, yearMax));

        return ToAPIGatewayProxyResponse(response);
    }
}