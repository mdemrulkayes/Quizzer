using Modules.AI.Core.Models;

namespace Modules.AI.Core.Repositories;

public interface IAIProviderConfigRepository
{
    Task<AIProviderConfig?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task SaveAsync(AIProviderConfig config, CancellationToken cancellationToken = default);
    Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
