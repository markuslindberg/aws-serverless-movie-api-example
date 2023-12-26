using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Tracing;
using MovieApi.Responses;

namespace MovieApi.Functions;

public abstract class RequestResponseFunctionBase
{
    protected IServiceProvider ServiceProvider { get; init; }
    protected JsonSerializerOptions JsonSerializerOptions { get; init; }

    protected RequestResponseFunctionBase(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        JsonSerializerOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    protected abstract Task<APIGatewayProxyResponse> HandleRequest(
        APIGatewayProxyRequest request,
        ILambdaContext context);

    [Tracing]
    [Logging(LogEvent = true)]
    [LambdaSerializer(typeof(SourceGeneratorLambdaJsonSerializer<HttpApiJsonSerializerContext>))]
    public async Task<APIGatewayProxyResponse> HandleAsync(APIGatewayProxyRequest request, ILambdaContext context)
    {
        return await HandleRequest(request, context);
    }

    protected APIGatewayProxyResponse ToAPIGatewayProxyResponse<T>(Response<T> response)
    {
        return new APIGatewayProxyResponse
        {
            StatusCode = response.StatusCode,
            Body = response.StatusCode == 200
                ? JsonSerializer.Serialize(response.Result, JsonSerializerOptions)
                : JsonSerializer.Serialize(
                    new
                    {
                        message = response.ErrorMessage
                    }, JsonSerializerOptions)
        };
    }
}

[JsonSerializable(typeof(APIGatewayProxyRequest))]
[JsonSerializable(typeof(APIGatewayProxyResponse))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public partial class HttpApiJsonSerializerContext : JsonSerializerContext
{
}