using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
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
        var category = request.QueryStringParameters["category"];
        var yearMin = ParseInt(request.QueryStringParameters["year-min"]);
        var yearMax = ParseInt(request.QueryStringParameters["year-max"]);

        var mediator = ServiceProvider.GetRequiredService<IMediator>();
        var response = await mediator.Send(new GetMoviesRequest(category, yearMin, yearMax));

        return ToAPIGatewayProxyResponse(response);
    }

    private int? ParseInt(string value) => int.TryParse(value, out var i) ? (int?)i : null;
}