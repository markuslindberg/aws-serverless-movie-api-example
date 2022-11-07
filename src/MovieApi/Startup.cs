using Amazon;
using Amazon.DynamoDBv2;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MovieApi.Logging;
using Serilog;
using Serilog.Formatting.Compact;

namespace MovieApi;

public static class Startup
{
    public static IServiceCollection Configure()
    {
        var services = new ServiceCollection();

        ConfigureServices(services);

        AWSSDKHandler.RegisterXRayForAllServices();
#if DEBUG
        AWSXRayRecorder.Instance.XRayOptions.IsXRayTracingDisabled = true;
#endif
        return services;
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        var configuration = BuildConfiguration();
        services.AddSingleton(configuration);

        var logger = CreateLogger();
        services.AddSingleton(logger);

        services.AddValidatorsFromAssemblyContaining(typeof(Startup));

        services.AddMediator();

        services.AddSingleton<IAmazonDynamoDB>(CreateAmazonDynamoDBClient());
    }

    private static IConfiguration BuildConfiguration() => new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true)
        .AddEnvironmentVariables()
        .Build();

    private static ILogger CreateLogger() => new LoggerConfiguration()
        .MinimumLevel.Information()
        .WriteTo.Console(new RenderedCompactJsonFormatter())
        .Enrich.With<AwsLambdaContextEnricher>()
        .Enrich.FromLogContext()
        .CreateLogger();

    private static AmazonDynamoDBClient CreateAmazonDynamoDBClient() => new AmazonDynamoDBClient(new AmazonDynamoDBConfig
    {
        RegionEndpoint = RegionEndpoint.GetBySystemName(Environment.GetEnvironmentVariable("AWS_REGION") ?? "eu-north-1")
    });
}