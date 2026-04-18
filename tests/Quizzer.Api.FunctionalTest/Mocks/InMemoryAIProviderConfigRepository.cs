using Modules.AI.Core.Models;
using Modules.AI.Core.Repositories;
using System.Collections.Concurrent;

namespace Quizzer.Api.FunctionalTest.Mocks;

public class InMemoryAIProviderConfigRepository : IAIProviderConfigRepository
{
    private readonly ConcurrentDictionary<Guid, AIProviderConfig> _configs = new();

    public Task<AIProviderConfig?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var config = _configs.Values.FirstOrDefault(c => c.UserId == userId && c.IsActive);
        return Task.FromResult(config);
    }

    public Task SaveAsync(AIProviderConfig config, CancellationToken cancellationToken = default)
    {
        _configs.AddOrUpdate(config.UserId, config, (_, _) => config);
        return Task.CompletedTask;
    }

    public Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var toRemove = _configs.Where(kvp => kvp.Value.UserId == userId).Select(kvp => kvp.Key).ToList();
        foreach (var key in toRemove)
            _configs.TryRemove(key, out _);
        return Task.CompletedTask;
    }
}
