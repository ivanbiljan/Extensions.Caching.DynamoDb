using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
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

    /// <inheritdoc />
    public byte[]? Get(string key)
    {
        return GetAsync(key).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task<byte[]?> GetAsync(string key, CancellationToken token = new())
    {
        token.ThrowIfCancellationRequested();

        var cacheEntry = await GetAndRefreshAsync(key, token);

        return cacheEntry?.Content;
    }

    /// <inheritdoc />
    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        SetAsync(key, value, options).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task SetAsync(
        string key,
        byte[] value,
        DistributedCacheEntryOptions options,
        CancellationToken token = new()
    )
    {
        var now = DateTimeOffset.Now;
        var slidingExpiration = options.SlidingExpiration ?? _options.DefaultSlidingExpiration;
        var expiresAt = options.AbsoluteExpiration ?? (options.AbsoluteExpirationRelativeToNow.HasValue
            ? now.Add(options.AbsoluteExpirationRelativeToNow.Value)
            : now.Add(slidingExpiration));

        var cacheEntry = new DynamoDbCacheEntry
        {
            Key = key,
            Content = value,
            ExpiresAt = expiresAt,
            SlidingExpiration = slidingExpiration,
            RowVersion = 0
        };

        await PersistToDynamoDb(cacheEntry, token);
    }

    /// <inheritdoc />
    public void Refresh(string key)
    {
        RefreshAsync(key).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task RefreshAsync(string key, CancellationToken token = new())
    {
        token.ThrowIfCancellationRequested();

        await GetAndRefreshAsync(key, token);
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
        await dynamoDb.DeleteItemAsync(deleteItemRequest, token);
    }

    private async Task PersistToDynamoDb(DynamoDbCacheEntry cacheEntry, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var cacheEntryAsAttributeMap = Document.FromJson(JsonSerializer.Serialize(cacheEntry)).ToAttributeMap();
        var putItemRequest = new PutItemRequest(_options.CacheTableName, cacheEntryAsAttributeMap)
        {
            ConditionExpression =
                $"attribute_not_exists({_options.PartitionKeyAttributeName}) OR rowVersion = :expectedRowVersion",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":expectedRowVersion"] = new()
                {
                    N = cacheEntry.RowVersion.ToString()
                }
            }
        };

        await dynamoDb.PutItemAsync(putItemRequest, token);
    }

    private async Task<DynamoDbCacheEntry?> GetAndRefreshAsync(string key, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var entryKey = new Dictionary<string, AttributeValue>
        {
            [_options.PartitionKeyAttributeName] = new(key)
        };

        var getItemRequest = new GetItemRequest(_options.CacheTableName, entryKey);
        var getItemResponse = await dynamoDb.GetItemAsync(getItemRequest, token);
        if (!getItemResponse.IsItemSet)
        {
            return null;
        }

        var cacheEntryAsDocument = Document.FromAttributeMap(getItemResponse.Item);
        var cacheEntry = JsonSerializer.Deserialize<DynamoDbCacheEntry>(cacheEntryAsDocument.ToJson())!;

        await RefreshAsync(cacheEntry, token);

        return cacheEntry.IsExpired() ? null : cacheEntry;
    }

    private async Task RefreshAsync(DynamoDbCacheEntry cacheEntry, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        
        if (cacheEntry is not {SlidingExpiration: not null})
        {
            return;
        }

        if (cacheEntry.ExpiresAt < DateTimeOffset.Now)
        {
            return;
        }

        cacheEntry.ExpiresAt = DateTimeOffset.Now.Add(cacheEntry.SlidingExpiration.Value);
        try
        {
            await PersistToDynamoDb(cacheEntry, token);
        }
        catch (ConditionalCheckFailedException ex)
        {
            Console.WriteLine(ex);
        }
    }
}