using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Modules.Quiz.Infrastructure.Data;
using Shared.Core;
using Shared.Core.Extensions;

namespace Modules.Quiz.Infrastructure.Persistence;
public class BaseRepository<TEntity>(QuestionModuleDbContext context) : IRepository<TEntity>
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

    public virtual async Task<List<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? expression = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _dbSet;
        if (include != null) query = include(query);
        if (expression != null) query = query.Where(expression);
        return await query.ToListAsync(cancellationToken);
    }

    public virtual async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> expression,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null)
    {
        IQueryable<TEntity> query = _dbSet;
        if (include != null) query = include(query);
        return await query.FirstOrDefaultAsync(expression);
    }

    public virtual async Task<PaginatedList<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? expression = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _dbSet;
        if (include != null) query = include(query);
        if (expression != null) query = query.Where(expression);
        return await query.ToPaginatedListAsync(pageNumber, pageSize, cancellationToken);
    }

    public virtual TEntity Update(TEntity entity)
    {
       return _dbSet.Update(entity).Entity;
    }
}
