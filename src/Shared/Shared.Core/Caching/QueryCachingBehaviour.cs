using MediatR;
using Microsoft.Extensions.Logging;

namespace Shared.Core.Caching;

/// <summary>
/// MediatR pipeline behaviour that caches responses for queries implementing ICacheableQuery.
/// </summary>
internal sealed class QueryCachingBehaviour<TRequest, TResponse>(
    ICacheService cacheService,
    ILogger<QueryCachingBehaviour<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheableQuery
    where TResponse : IBaseResult
{
    private static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromMinutes(5);

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var cacheKey = request.CacheKey;

        var cached = await cacheService.GetAsync<TResponse>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            logger.LogDebug("Cache hit for {CacheKey}", cacheKey);
            return cached;
        }

        logger.LogDebug("Cache miss for {CacheKey}, executing handler", cacheKey);

        var response = await next();

        if (response.IsSuccess)
        {
            var duration = request.CacheDuration ?? DefaultCacheDuration;
            await cacheService.SetAsync(cacheKey, response, duration, cancellationToken);
        }

        return response;
    }
}
