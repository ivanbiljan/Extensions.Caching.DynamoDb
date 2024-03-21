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
    ///     Gets the absolute expiration date for this entry.
    /// </summary>
    [JsonPropertyName("expiresAtUtc")]
    public required DateTimeOffset? AbsoluteExpiration { get; init; }

    /// <summary>
    ///     Gets an interval that defines how long the entry can be inactive before it is removed.
    /// </summary>
    [JsonPropertyName("slidingExpiration")]
    public required TimeSpan? SlidingExpiration { get; init; }
}