using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Shared.Core.Caching;
using StackExchange.Redis;

namespace Shared.Infrastructure.Caching;

internal sealed class RedisCacheService(
    IDistributedCache distributedCache,
    IConnectionMultiplexer connectionMultiplexer,
    ILogger<RedisCacheService> logger) : ICacheService
{
    private const string InstancePrefix = "Quizzer:";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        Converters = { new ResultJsonConverterFactory() }
    };

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var cached = await distributedCache.GetStringAsync(key, cancellationToken);
            if (cached is null)
                return default;

            return JsonSerializer.Deserialize<T>(cached, JsonOptions);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to read cache key {CacheKey}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5)
            };

            var json = JsonSerializer.Serialize(value, JsonOptions);
            await distributedCache.SetStringAsync(key, json, options, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to set cache key {CacheKey}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await distributedCache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to remove cache key {CacheKey}", key);
        }
    }

    public async Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var pattern = $"{InstancePrefix}{prefixKey}*";
            var endpoints = connectionMultiplexer.GetEndPoints();
            var db = connectionMultiplexer.GetDatabase();

            foreach (var endpoint in endpoints)
            {
                var server = connectionMultiplexer.GetServer(endpoint);
                await foreach (var key in server.KeysAsync(pattern: pattern))
                {
                    await db.KeyDeleteAsync(key);
                    logger.LogDebug("Removed cache key {CacheKey} by prefix {Prefix}", key, prefixKey);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to remove cache keys by prefix {Prefix}", prefixKey);
        }
    }
}
