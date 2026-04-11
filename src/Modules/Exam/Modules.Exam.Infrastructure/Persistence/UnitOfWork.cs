using Shared.Core;

namespace Modules.Exam.Infrastructure.Persistence;

internal sealed class UnitOfWork(ExamModuleDbContext dbContext) : IUnitOfWork, IDisposable
{
    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        dbContext.Dispose();
    }
}
