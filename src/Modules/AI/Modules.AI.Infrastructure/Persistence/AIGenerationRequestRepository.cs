using Modules.AI.Core.Models;
using Modules.AI.Core.Repositories;
using Modules.AI.Infrastructure.Data;
using MongoDB.Driver;

namespace Modules.AI.Infrastructure.Persistence;

public class AIGenerationRequestRepository(AIModuleMongoContext context) : IAIGenerationRequestRepository
{
    public async Task<AIGenerationRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.GenerationRequests
            .Find(r => r.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<AIGenerationRequest>> GetByUserIdAsync(
        Guid userId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        return await context.GenerationRequests
            .Find(r => r.UserId == userId)
            .SortByDescending(r => r.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveAsync(AIGenerationRequest request, CancellationToken cancellationToken = default)
    {
        await context.GenerationRequests.InsertOneAsync(request, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(AIGenerationRequest request, CancellationToken cancellationToken = default)
    {
        await context.GenerationRequests.ReplaceOneAsync(
            r => r.Id == request.Id, request, cancellationToken: cancellationToken);
    }
}
