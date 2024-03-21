using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Extensions.Caching.DynamoDb;

/// <summary>
///     Provides DynamoDbCache extension methods for the <see cref="IServiceCollection" /> type.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds the services required for the DynamoDb distributed cache to the provided <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" />.</param>
    /// <param name="options">The action used to configure the options.</param>
    /// <returns>The modified <see cref="IServiceCollection" />.</returns>
    public static IServiceCollection AddDynamoDbCache(
        this IServiceCollection services,
        Action<DynamoDbCacheOptions> options
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        services.AddOptions();
        services.AddSingleton<IValidateOptions<DynamoDbCacheOptions>, DynamoDbCacheOptionsValidator>();
        services.Configure(options);

        services.AddSingleton<IDistributedCache, DynamoDbCache>();

        return services;
    }
}