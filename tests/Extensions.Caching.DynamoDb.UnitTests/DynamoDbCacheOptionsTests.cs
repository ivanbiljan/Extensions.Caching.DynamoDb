using Amazon.Internal;
using Microsoft.Extensions.Options;

namespace Extensions.Caching.DynamoDb.UnitTests;

public sealed class DynamoDbCacheOptionsTests
{
    [Fact]
    public void MissingRegion_ReturnsError()
    {
        var validator = new DynamoDbCacheOptionsValidator();
        var options = new DynamoDbCacheOptions();

        var result = validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Single(result.Failures!);
    }
    
    [Fact]
    public void IncompleteAccessKey_ReturnsError()
    {
        var validator = new DynamoDbCacheOptionsValidator();
        var options = new DynamoDbCacheOptions
        {
            Region = "us-east1",
            AccessKeyId = "Access key ID"
        };

        var result = validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Single(result.Failures!);
    }
    
    [Fact]
    public void IncompleteSecretKey_ReturnsError()
    {
        var validator = new DynamoDbCacheOptionsValidator();
        var options = new DynamoDbCacheOptions
        {
            Region = "us-east1",
            SecretKey = "Secret key"
        };

        var result = validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Single(result.Failures!);
    }
}