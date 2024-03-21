using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime.Internal;
using Extensions.Caching.DynamoDb.Internal;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Extensions.Caching.DynamoDb;

/// <summary>
///     Represents an implementation of <see cref="IDistributedCache" /> that uses DynamoDb as backing storage.
/// </summary>
public sealed class DynamoDbCache(IOptions<DynamoDbCacheOptions> options, IAmazonDynamoDB dynamoDb) : IDistributedCache
{
    private readonly DynamoDbCacheOptions _options = options.Value;
    private readonly IAmazonDynamoDB _dynamoDb = dynamoDb;

    /// <inheritdoc />
    public byte[]? Get(string key)
    {
        return GetAsync(key).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task<byte[]?> GetAsync(string key, CancellationToken token = new())
    {
        token.ThrowIfCancellationRequested();
        
        var entryKey = new Dictionary<string, AttributeValue>
        {
            [_options.PartitionKeyAttributeName] = new(key)
        };

        var getItemRequest = new GetItemRequest(_options.CacheTableName, entryKey);
        var getItemResponse = await _dynamoDb.GetItemAsync(getItemRequest, token);
        if (!getItemResponse.IsItemSet)
        {
            return null;
        }

        var cacheEntryAsDocument = Document.FromAttributeMap(getItemResponse.Item);
        var cacheEntry = JsonSerializer.Deserialize<DynamoDbCacheEntry>(cacheEntryAsDocument.ToJson())!;

        return cacheEntry.Content;
    }

    /// <inheritdoc />
    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        SetAsync(key, value, options).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public Task SetAsync(
        string key,
        byte[] value,
        DistributedCacheEntryOptions options,
        CancellationToken token = new()
    )
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void Refresh(string key)
    {
        RefreshAsync(key).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public Task RefreshAsync(string key, CancellationToken token = new())
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void Remove(string key)
    {
        RemoveAsync(key).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key, CancellationToken token = new())
    {
        token.ThrowIfCancellationRequested();
        
        var entryKey = new Dictionary<string, AttributeValue>
        {
            [_options.PartitionKeyAttributeName] = new(key)
        };

        var deleteItemRequest = new DeleteItemRequest(_options.CacheTableName, entryKey);
        await _dynamoDb.DeleteItemAsync(deleteItemRequest, token);
    }
}