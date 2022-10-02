using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using MovieApi.Tests.Infrastructure;

namespace MovieApi.Tests.Functions;

public class DynamoDbFixture : IAsyncLifetime, IDisposable
{
    private readonly TestcontainersContainer _dynamoDbLocal = new TestcontainersBuilder<TestcontainersContainer>()
        .WithImage("amazon/dynamodb-local:1.18.0")
        .WithCommand("-jar", "DynamoDBLocal.jar", "-inMemory", "-sharedDb")
        .WithPortBinding(8000)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8000))
        .Build();

    public DynamoDbFixture()
    {
    }

    public void DynamoDbSetup(bool seed = true)
    {
        var cd = Directory.GetCurrentDirectory();
        var wd = Path.Combine(cd.Substring(0, cd.IndexOf("test/MovieApi.Tests")), "src/MovieApi");
        var b = new Bash(wd);

        b.Run("npm run migrate");

        if (seed)
        {
            b.Run("npm run seed:local");
        }
    }

    public async Task InitializeAsync()
    {
        await _dynamoDbLocal.StartAsync();
        
        DynamoDbSetup();
    }

    public Task DisposeAsync()
    {
        return this._dynamoDbLocal.DisposeAsync().AsTask();
    }

    public void Dispose()
    {
    }
}