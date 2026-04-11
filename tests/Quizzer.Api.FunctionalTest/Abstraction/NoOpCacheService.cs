using Shared.Core.Caching;

namespace Quizzer.Api.FunctionalTest.Abstraction;

internal sealed class NoOpCacheService : ICacheService
{
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        => Task.FromResult(default(T));

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
