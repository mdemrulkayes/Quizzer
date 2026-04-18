using Modules.AI.Core.Models;
using Modules.AI.Core.Repositories;
using Modules.AI.Infrastructure.Data;
using MongoDB.Driver;

namespace Modules.AI.Infrastructure.Persistence;

public class InterviewPrepMaterialRepository(AIModuleMongoContext context) : IInterviewPrepMaterialRepository
{
    public async Task<InterviewPrepMaterial?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.InterviewPrepMaterials
            .Find(m => m.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<InterviewPrepMaterial>> GetByUserIdAsync(
        Guid userId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        return await context.InterviewPrepMaterials
            .Find(m => m.UserId == userId)
            .SortByDescending(m => m.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveAsync(InterviewPrepMaterial material, CancellationToken cancellationToken = default)
    {
        await context.InterviewPrepMaterials.InsertOneAsync(material, cancellationToken: cancellationToken);
    }
}
