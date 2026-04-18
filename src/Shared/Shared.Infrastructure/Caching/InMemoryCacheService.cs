using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Shared.Core.Caching;

namespace Shared.Infrastructure.Caching;

internal sealed class InMemoryCacheService(
    IMemoryCache memoryCache,
    ILogger<InMemoryCacheService> logger) : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        Converters = { new ResultJsonConverterFactory() }
    };

    // Maps prefix → all cache keys that were stored under that prefix
    private readonly ConcurrentDictionary<string, ConcurrentBag<string>> _prefixIndex = new();

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!memoryCache.TryGetValue(key, out string? json) || json is null)
                return Task.FromResult(default(T));

            var result = JsonSerializer.Deserialize<T>(json, JsonOptions);
            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to read in-memory cache key {CacheKey}", key);
            return Task.FromResult(default(T));
        }
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(value, JsonOptions);
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5)
            };

            memoryCache.Set(key, json, options);
            IndexKeyByPrefixes(key);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to set in-memory cache key {CacheKey}", key);
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            memoryCache.Remove(key);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to remove in-memory cache key {CacheKey}", key);
        }

        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_prefixIndex.TryGetValue(prefixKey, out var keys))
                return Task.CompletedTask;

            foreach (var key in keys)
            {
                memoryCache.Remove(key);
                logger.LogDebug("Removed in-memory cache key {CacheKey} by prefix {Prefix}", key, prefixKey);
            }

            // Replace the bag so stale keys don't accumulate
            _prefixIndex[prefixKey] = new ConcurrentBag<string>();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to remove in-memory cache keys by prefix {Prefix}", prefixKey);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Registers the key under every prefix segment it starts with,
    /// enabling prefix-based invalidation via <see cref="RemoveByPrefixAsync"/>.
    /// </summary>
    private void IndexKeyByPrefixes(string key)
    {
        // A cache key typically looks like "tags:list" or "question-sets:42".
        // We index under every leading segment separated by ':'.
        var parts = key.Split(':');
        var prefix = string.Empty;
        foreach (var part in parts)
        {
            prefix = prefix.Length == 0 ? part : $"{prefix}:{part}";
            _prefixIndex.GetOrAdd(prefix, _ => new ConcurrentBag<string>()).Add(key);
        }
    }
}
