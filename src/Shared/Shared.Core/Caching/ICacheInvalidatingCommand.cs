namespace Shared.Core.Caching;

/// <summary>
/// Marker interface for commands that should invalidate cached data.
/// Implement this on ICommand records to trigger automatic cache invalidation after execution.
/// </summary>
public interface ICacheInvalidatingCommand
{
    /// <summary>
    /// Cache keys to remove after the command succeeds.
    /// Use exact keys for specific entries (e.g., "tags:id:5") 
    /// or prefix keys for bulk invalidation (e.g., "tags:all").
    /// </summary>
    string[] CacheKeysToInvalidate { get; }
}
