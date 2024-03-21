using Microsoft.Extensions.Caching.Distributed;

namespace Extensions.Caching.DynamoDb;

/// <summary>
///     Represents an implementation of <see cref="IDistributedCache" /> that uses DynamoDb as backing storage.
/// </summary>
public sealed class DynamoDbCache : IDistributedCache
{
    public byte[]? Get(string key)
    {
        throw new NotImplementedException();
    }

    public Task<byte[]?> GetAsync(string key, CancellationToken token = new())
    {
        throw new NotImplementedException();
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        throw new NotImplementedException();
    }

    public Task SetAsync(
        string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = new()
    )
    {
        throw new NotImplementedException();
    }

    public void Refresh(string key)
    {
        throw new NotImplementedException();
    }

    public Task RefreshAsync(string key, CancellationToken token = new())
    {
        throw new NotImplementedException();
    }

    public void Remove(string key)
    {
        throw new NotImplementedException();
    }

    public Task RemoveAsync(string key, CancellationToken token = new())
    {
        throw new NotImplementedException();
    }
}