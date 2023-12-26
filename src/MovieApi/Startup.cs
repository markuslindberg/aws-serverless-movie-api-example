using Amazon;
using Amazon.DynamoDBv2;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        services.AddValidatorsFromAssemblyContaining(typeof(Startup));

        services.AddMediator();

        services.AddSingleton<IAmazonDynamoDB>(CreateAmazonDynamoDBClient());
    }

    private static IConfiguration BuildConfiguration() => new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true)
        .AddEnvironmentVariables()
        .Build();

    private static AmazonDynamoDBClient CreateAmazonDynamoDBClient() => new AmazonDynamoDBClient(new AmazonDynamoDBConfig
    {
        RegionEndpoint = RegionEndpoint.GetBySystemName(Environment.GetEnvironmentVariable("AWS_REGION") ?? "eu-north-1")
    });
}