using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using AWS.Lambda.Powertools.Logging;
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

    [LambdaSerializer(typeof(SourceGeneratorLambdaJsonSerializer<HttpApiJsonSerializerContext>))]
    public async Task<APIGatewayProxyResponse> HandleAsync(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var sw = Stopwatch.StartNew();

        Logger.AppendKeys(new Dictionary<string, object>()
            {
                { "RequestPath", request.Path ?? string.Empty }
            });

        try
        {
            var response = await HandleRequest(request, context);

            Logger
                .LogInformation("Function completed in {ElapsedMilliseconds} ms", sw.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            Logger
                .LogError(ex, "Function failed after {ElapsedMilliseconds} ms", sw.ElapsedMilliseconds);

            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = JsonSerializer.Serialize(
                    new
                    {
                        message = "Function failed with exception"
                    }, JsonSerializerOptions)
            };
        }
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