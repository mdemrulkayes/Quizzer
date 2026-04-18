using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Core;
using Shared.Core.Caching;
using Shared.Core.IntegrationEvents;
using Shared.Infrastructure.Caching;
using Shared.Infrastructure.IntegrationEvents;
using StackExchange.Redis;

namespace Shared.Infrastructure;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterSharedInfrastructureModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITimeProvider, TimeProvider>();

        // Integration event bus
        services.AddSingleton<IntegrationEventChannel>();
        services.AddScoped<IIntegrationEventPublisher, IntegrationEventPublisher>();
        services.AddHostedService<IntegrationEventProcessor>();

        RegisterCacheServices(services, configuration);

        return services;
    }

    private static void RegisterCacheServices(IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";

        IConnectionMultiplexer? redis = null;
        try
        {
            var redisConfig = ConfigurationOptions.Parse(redisConnectionString);
            redisConfig.AbortOnConnectFail = false;
            redisConfig.ConnectTimeout = 3000;

            redis = ConnectionMultiplexer.Connect(redisConfig);

            // Verify the connection is actually usable
            redis.GetDatabase().Ping();
        }
        catch (Exception)
        {
            redis?.Dispose();
            redis = null;
        }

        if (redis is not null)
        {
            services.AddSingleton<IConnectionMultiplexer>(redis);
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "Quizzer:";
            });
            services.AddSingleton<ICacheService, RedisCacheService>();
        }
        else
        {
            // Redis unavailable — fall back to in-memory cache and log on first resolution
            services.AddMemoryCache();
            services.AddSingleton<ICacheService>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<InMemoryCacheService>>();
                logger.LogWarning(
                    "Redis is not available. Falling back to in-memory cache. " +
                    "Data cached in memory will not be shared across instances and will be lost on restart.");
                return new InMemoryCacheService(
                    sp.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>(),
                    logger);
            });
        }
    }
}
