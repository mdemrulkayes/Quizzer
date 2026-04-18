using Modules.AI.Core.Models;
using Modules.AI.Core.Repositories;
using System.Collections.Concurrent;

namespace Quizzer.Api.FunctionalTest.Mocks;

public class InMemoryInterviewPrepMaterialRepository : IInterviewPrepMaterialRepository
{
    private readonly ConcurrentDictionary<Guid, InterviewPrepMaterial> _materials = new();

    public Task<InterviewPrepMaterial?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _materials.TryGetValue(id, out var material);
        return Task.FromResult(material);
    }

    public Task<List<InterviewPrepMaterial>> GetByUserIdAsync(Guid userId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var results = _materials.Values
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return Task.FromResult(results);
    }

    public Task SaveAsync(InterviewPrepMaterial material, CancellationToken cancellationToken = default)
    {
        _materials.AddOrUpdate(material.Id, material, (_, _) => material);
        return Task.CompletedTask;
    }
}
