using Modules.AI.Core.Models;
using Modules.AI.Core.Repositories;
using System.Collections.Concurrent;

namespace Quizzer.Api.FunctionalTest.Mocks;

public class InMemoryAIGenerationRequestRepository : IAIGenerationRequestRepository
{
    private readonly ConcurrentDictionary<Guid, AIGenerationRequest> _requests = new();

    public Task<AIGenerationRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _requests.TryGetValue(id, out var request);
        return Task.FromResult(request);
    }

    public Task<List<AIGenerationRequest>> GetByUserIdAsync(Guid userId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var results = _requests.Values
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return Task.FromResult(results);
    }

    public Task SaveAsync(AIGenerationRequest request, CancellationToken cancellationToken = default)
    {
        _requests.AddOrUpdate(request.Id, request, (_, _) => request);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(AIGenerationRequest request, CancellationToken cancellationToken = default)
    {
        _requests.AddOrUpdate(request.Id, request, (_, _) => request);
        return Task.CompletedTask;
    }
}
