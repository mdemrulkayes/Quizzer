using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Shared.Core;
using Shared.Core.Extensions;

namespace Modules.Exam.Infrastructure.Persistence.Repositories;

public class BaseRepository<TEntity>(ExamModuleDbContext context) : IRepository<TEntity>
    where TEntity : BaseEntity
{
    private readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();

    public virtual TEntity Add(TEntity entity)
    {
        return _dbSet.Add(entity).Entity;
    }

    public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> expression)
    {
        return await _dbSet.AnyAsync(expression);
    }

    public virtual TEntity Delete(TEntity entity)
    {
        return _dbSet.Remove(entity).Entity;
    }

    public virtual async Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? expression = null, CancellationToken cancellationToken = default)
    {
        if (expression != null)
            return await _dbSet.Where(expression).ToListAsync(cancellationToken);
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> expression, string includeProperties = "")
    {
        return await _dbSet.FirstOrDefaultAsync(expression);
    }

    public virtual async Task<PaginatedList<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? expression = null, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        if (expression != null)
            return await _dbSet.Where(expression).ToPaginatedListAsync(pageNumber, pageSize, cancellationToken);
        return await _dbSet.ToPaginatedListAsync(pageNumber, pageSize, cancellationToken);
    }

    public virtual TEntity Update(TEntity entity)
    {
        return _dbSet.Update(entity).Entity;
    }
}
