using MediatR;
using Microsoft.Extensions.Logging;

namespace Shared.Core.Caching;

/// <summary>
/// MediatR pipeline behaviour that invalidates cache entries after commands implementing ICacheInvalidatingCommand succeed.
/// </summary>
internal sealed class CacheInvalidationBehaviour<TRequest, TResponse>(
    ICacheService cacheService,
    ILogger<CacheInvalidationBehaviour<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheInvalidatingCommand
    where TResponse : IBaseResult
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();

        if (!response.IsSuccess)
            return response;

        foreach (var key in request.CacheKeysToInvalidate)
        {
            logger.LogDebug("Invalidating cache by prefix {CacheKey}", key);
            await cacheService.RemoveByPrefixAsync(key, cancellationToken);
        }

        return response;
    }
}
