using Amazon;
using Amazon.Internal;
using Microsoft.Extensions.Options;

namespace Extensions.Caching.DynamoDb;

/// <summary>
///     Defines configuration options for the <see cref="DynamoDbCache" />.
/// </summary>
public sealed record DynamoDbCacheOptions
{
    /// <summary>
    ///     Identifies the section used to bind these options from a configuration source.
    /// </summary>
    public const string ConfigurationSectionName = "DynamoDbCache";

    /// <summary>
    ///     Gets the AWS region.
    /// </summary>
    public string Region { get; init; } = string.Empty;

    /// <summary>
    ///     Gets the access key ID component of the access key pair used to authenticate requests towards AWS.
    /// </summary>
    public string? AccessKeyId { get; init; }

    /// <summary>
    ///     Gets the secret access key component of the access key pair used to authenticate requests towards AWS.
    /// </summary>
    public string? SecretKey { get; init; }

    /// <summary>
    ///     Gets the endpoint DynamoDb will connect to.
    /// </summary>
    public string? ServiceUrl { get; init; }

    /// <summary>
    ///     Gets the name of the table cache entries will be written to. Defaults to <c>distributed-cache</c>.
    /// </summary>
    public string CacheTableName { get; init; } = "distributed-cache";

    /// <summary>
    ///     Gets the name of the partition key attribute. Defaults to <c>pk</c>.
    /// </summary>
    public string PartitionKeyAttributeName { get; init; } = "pk";

    /// <summary>
    ///     Gets the sliding expiration used when a cache entry does not have an explicit expiration set.
    /// </summary>
    public TimeSpan DefaultSlidingExpiration { get; init; } = TimeSpan.FromMinutes(15);
}

internal sealed class DynamoDbCacheOptionsValidator : IValidateOptions<DynamoDbCacheOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, DynamoDbCacheOptions options)
    {
        var failures = new List<string>();
        if (string.IsNullOrWhiteSpace(options.Region) && string.IsNullOrWhiteSpace(options.ServiceUrl))
        {
            failures.Add("Either region or service URL should be set");
        }

        if ((!string.IsNullOrWhiteSpace(options.AccessKeyId) && string.IsNullOrWhiteSpace(options.SecretKey)) ||
            (string.IsNullOrWhiteSpace(options.AccessKeyId) && !string.IsNullOrWhiteSpace(options.SecretKey)))
        {
            failures.Add("Incomplete access key");
        }

        return failures.Count != 0 ? ValidateOptionsResult.Fail(failures) : ValidateOptionsResult.Success;
    }
}