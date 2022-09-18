using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Context;

namespace MovieApi.Functions;

public abstract class RequestResponseFunctionBase
{
    private bool _isColdStart = true;
    protected IServiceProvider ServiceProvider { get; init; }
    protected ILogger Logger { get; init; }
    protected JsonSerializerOptions JsonSerializerOptions { get; init; }

    protected RequestResponseFunctionBase(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        Logger = ServiceProvider.GetRequiredService<ILogger>();
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
        using (LogContext.PushProperty("RequestPath", request.Path))
        using (LogContext.PushProperty("RequestId", context.AwsRequestId))
        using (LogContext.PushProperty("FunctionArn", context.InvokedFunctionArn))
        using (LogContext.PushProperty("ColdStart", _isColdStart))
        {
            _isColdStart = false;
            var sw = Stopwatch.StartNew();

            try
            {
                var response = await HandleRequest(request, context);

                Logger
                    .ForContext("Request", JsonSerializer.Serialize(request, JsonSerializerOptions))
                    .ForContext("Response", JsonSerializer.Serialize(response, JsonSerializerOptions))
                    .Information("Function completed in {ElapsedMilliseconds} ms", sw.ElapsedMilliseconds);

                return response;
            }
            catch (Exception ex)
            {
                Logger
                    .ForContext("Request", JsonSerializer.Serialize(request, JsonSerializerOptions))
                    .Error(ex, "Function failed after {ElapsedMilliseconds} ms", sw.ElapsedMilliseconds);

                return new APIGatewayProxyResponse
                {
                    Body = "Function failed with exception",
                    StatusCode = 500
                };
            }
        }
    }
}

[JsonSerializable(typeof(APIGatewayProxyRequest))]
[JsonSerializable(typeof(APIGatewayProxyResponse))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public partial class HttpApiJsonSerializerContext : JsonSerializerContext
{
}