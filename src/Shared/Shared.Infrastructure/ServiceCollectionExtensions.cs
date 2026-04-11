using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        // Redis connection
        var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(redisConnectionString));

        // Distributed cache (Redis)
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "Quizzer:";
        });
        services.AddSingleton<ICacheService, RedisCacheService>();

        return services;
    }
}
