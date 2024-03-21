using System.Security.Cryptography;
using Amazon.DynamoDBv2;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Extensions.Caching.DynamoDb.IntegrationTests;

public sealed class DynamoDbLocalTests
{
    private const string ServiceUrl = "http://localhost:8000";
    
    private readonly DynamoDbCacheOptions _options = new()
    {
        Region = "localhost",
        AccessKeyId = "fakeAccessKey",
        SecretKey = "fakeSecretKey"
    };
    
    private readonly IAmazonDynamoDB _dynamoDb = new AmazonDynamoDBClient(
        new AmazonDynamoDBConfig
        {
            ServiceURL = ServiceUrl
        }
    );

    [Fact]
    public void ItemDoesNotExist_ReturnsNull()
    {
        var cache = CreateCache();

        var item = cache.Get("does_not_exist");
        
        Assert.Null(item);
    }

    [Fact]
    public void NoSlidingOrAbsoluteExpiration_InsertsSuccessfully()
    {
        var cache = CreateCache();

        var content = RandomNumberGenerator.GetBytes(64);

        cache.Set("exists", content, new DistributedCacheEntryOptions());
        var result = cache.Get("exists");
        
        Assert.Equal(content, result);

        cache.Remove("exists");
    }

    private DynamoDbCache CreateCache()
    {
        return new DynamoDbCache(new OptionsWrapper<DynamoDbCacheOptions>(_options), _dynamoDb);
    }
}