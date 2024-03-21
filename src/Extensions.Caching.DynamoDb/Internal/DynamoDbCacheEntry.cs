using System.Text.Json.Serialization;

namespace Extensions.Caching.DynamoDb.Internal;

/// <summary>
///     Represents a cache item that is persisted to DynamoDb.
/// </summary>
internal sealed record DynamoDbCacheEntry
{
    /// <summary>
    ///     Gets the cache key used for lookup.
    /// </summary>
    [JsonPropertyName("pk")]
    public required string Key { get; init; }

    /// <summary>
    ///     Gets the content.
    /// </summary>
    [JsonPropertyName("content")]
    public required byte[] Content { get; init; }

    /// <summary>
    ///     Gets the expiration date for this entry.
    /// </summary>
    [JsonPropertyName("expiresAtUtc")]
    public required DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>
    ///     Gets an interval that defines how long the entry can be inactive before it is removed. Sliding expiration entries
    ///     will never exceed <see cref="AbsoluteExpiration" /> if one is set.
    /// </summary>
    [JsonPropertyName("slidingExpiration")]
    public required TimeSpan? SlidingExpiration { get; init; }

    /// <summary>
    ///     Gets the version number acquired from a read operation. Used for optimistic locking.
    /// </summary>
    [JsonPropertyName("rowVersion")]
    public required int RowVersion { get; init; }

    /// <summary>
    ///     Returns a value indicating whether the item has expired.
    /// </summary>
    /// <returns><c>true</c> if the item expired, <c>false</c> otherwise.</returns>
    public bool IsExpired()
    {
        return ExpiresAt <= DateTimeOffset.Now;
    }
}