using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace Extensions.Caching.DynamoDb.UnitTests.Extensions;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDynamoDbCache_NullServiceCollection_ThrowsArgumentNullException()
    {
        IServiceCollection services = null!;

        Assert.Throws<ArgumentNullException>(() => ServiceCollectionExtensions.AddDynamoDbCache(services, _ => { }));
    }

    [Fact]
    public void AddDynamoDbCache_NullSetupAction_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() => services.AddDynamoDbCache(null!));
    }
    
    [Fact]
    public void AddDynamoDbCache_AllowsChaining()
    {
        var services = new ServiceCollection();

        Assert.Same(services, services.AddDynamoDbCache(_ => { }));
    }
}