namespace Shared.Core.Caching;

/// <summary>
/// Marker interface for queries that should be cached.
/// Implement this on IQuery records to opt into the caching pipeline.
/// </summary>
public interface ICacheableQuery
{
    /// <summary>
    /// Unique cache key for this query instance.
    /// </summary>
    string CacheKey { get; }

    /// <summary>
    /// Time-to-live for the cached entry. Null uses the default (5 minutes).
    /// </summary>
    TimeSpan? CacheDuration => null;
}
