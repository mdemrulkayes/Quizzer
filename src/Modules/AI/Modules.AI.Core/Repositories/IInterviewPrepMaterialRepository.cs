using Modules.AI.Core.Models;

namespace Modules.AI.Core.Repositories;

public interface IInterviewPrepMaterialRepository
{
    Task<InterviewPrepMaterial?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<InterviewPrepMaterial>> GetByUserIdAsync(Guid userId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task SaveAsync(InterviewPrepMaterial material, CancellationToken cancellationToken = default);
}
