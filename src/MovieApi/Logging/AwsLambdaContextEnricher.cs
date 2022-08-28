using Serilog.Core;
using Serilog.Events;

namespace MovieApi.Logging;

public class AwsLambdaContextEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        AddIfNotNull(logEvent, propertyFactory.CreateProperty("XRayTraceId", XRayTraceId));
        AddIfNotNull(logEvent, propertyFactory.CreateProperty("AwsRegion", GetEnv("AWS_REGION")));
        AddIfNotNull(logEvent, propertyFactory.CreateProperty("FunctionName", GetEnv("AWS_LAMBDA_FUNCTION_NAME")));
        AddIfNotNull(logEvent, propertyFactory.CreateProperty("FunctionVersion", GetEnv("AWS_LAMBDA_FUNCTION_VERSION")));
        AddIfNotNull(logEvent, propertyFactory.CreateProperty("FunctionMemorySize", GetEnv("AWS_LAMBDA_FUNCTION_MEMORY_SIZE")));
        AddIfNotNull(logEvent, propertyFactory.CreateProperty("ExecutionEnvironment", GetEnv("AWS_EXECUTION_ENV")));
        AddIfNotNull(logEvent, propertyFactory.CreateProperty("LogGroupName", GetEnv("AWS_LAMBDA_LOG_GROUP_NAME")));
        AddIfNotNull(logEvent, propertyFactory.CreateProperty("LogStreamName", GetEnv("AWS_LAMBDA_LOG_STREAM_NAME")));
    }

    private void AddIfNotNull(LogEvent e, LogEventProperty p)
    {
        if (p != null)
        {
            e.AddPropertyIfAbsent(p);
        }
    }

    private string? GetEnv(string key) => Environment.GetEnvironmentVariable(key);

    private string? XRayTraceId => Environment.GetEnvironmentVariable("_X_AMZN_TRACE_ID")
            ?.Split(';', StringSplitOptions.RemoveEmptyEntries)[0][5..];
}