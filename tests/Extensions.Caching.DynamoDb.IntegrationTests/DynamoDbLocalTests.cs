using System.Reflection;
using System.Security.Cryptography;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Xunit.Sdk;

namespace Extensions.Caching.DynamoDb.IntegrationTests;

public sealed class DynamoDbLocalTests : IDisposable
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

    public DynamoDbLocalTests()
    {
        try
        {
            var createTableRequest = new CreateTableRequest(
                _options.CacheTableName,
                [
                    new KeySchemaElement(_options.PartitionKeyAttributeName, KeyType.HASH)
                ]
            )
            {
                AttributeDefinitions =
                [
                    new AttributeDefinition(_options.PartitionKeyAttributeName, ScalarAttributeType.S)
                ],
                ProvisionedThroughput = new ProvisionedThroughput(1024, 1024)
            };

            _dynamoDb.CreateTableAsync(createTableRequest).GetAwaiter().GetResult();
        }
        catch
        {
            // ignored
        }
    }

    [Fact]
    public void Get_ItemDoesNotExist_ReturnsNull()
    {
        var cache = CreateCache();

        var item = cache.Get("does_not_exist");
        
        Assert.Null(item);
    }

    [Fact]
    public void Set_AbsoluteExpirationInThePast_ThrowsArgumentException()
    {
        var cache = CreateCache();

        Assert.Throws<ArgumentException>(
            () => cache.Set(
                "throws",
                RandomNumberGenerator.GetBytes(8),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddDays(-7)
                }
            )
        );
    }

    [Fact]
    public void Set_NoSlidingOrAbsoluteExpiration_InsertsSuccessfully()
    {
        var cache = CreateCache();

        var content = RandomNumberGenerator.GetBytes(64);
        cache.Set("exists", content, new DistributedCacheEntryOptions());
        var result = cache.Get("exists");
        
        Assert.Equal(content, result);

        cache.Remove("exists");
    }

    [Fact]
    public void Remove_ItemDoesNotExist_DoesNothing()
    {
        var cache = CreateCache();

        cache.Remove("does_not_exist");
    }

    private DynamoDbCache CreateCache()
    {
        return new DynamoDbCache(new OptionsWrapper<DynamoDbCacheOptions>(_options), _dynamoDb);
    }

    public void Dispose()
    {
        try
        {
            var deleteTableRequest = new DeleteTableRequest(_options.CacheTableName);
            _dynamoDb.DeleteTableAsync(deleteTableRequest).GetAwaiter().GetResult();
        }
        catch
        {
            // ignored
        }
        
        _dynamoDb.Dispose();
    }
}