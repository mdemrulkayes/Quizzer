using Modules.AI.Core.Models;

namespace Modules.AI.Core.Repositories;

public interface IAIGenerationRequestRepository
{
    Task<AIGenerationRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<AIGenerationRequest>> GetByUserIdAsync(Guid userId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task SaveAsync(AIGenerationRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(AIGenerationRequest request, CancellationToken cancellationToken = default);
}
