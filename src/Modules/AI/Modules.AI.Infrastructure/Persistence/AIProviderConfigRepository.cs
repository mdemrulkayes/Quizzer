using Modules.AI.Core.Models;
using Modules.AI.Core.Repositories;
using Modules.AI.Infrastructure.Data;
using MongoDB.Driver;

namespace Modules.AI.Infrastructure.Persistence;

public class AIProviderConfigRepository(AIModuleMongoContext context) : IAIProviderConfigRepository
{
    public async Task<AIProviderConfig?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.ProviderConfigs
            .Find(c => c.UserId == userId && c.IsActive)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task SaveAsync(AIProviderConfig config, CancellationToken cancellationToken = default)
    {
        var filter = Builders<AIProviderConfig>.Filter.Eq(c => c.UserId, config.UserId);
        await context.ProviderConfigs.ReplaceOneAsync(
            filter, config, new ReplaceOptions { IsUpsert = true }, cancellationToken);
    }

    public async Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await context.ProviderConfigs.DeleteManyAsync(
            c => c.UserId == userId, cancellationToken);
    }
}
