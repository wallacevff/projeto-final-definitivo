using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using ProjetoFinal.Domain.Shared.Filters;
using ProjetoFinal.Domain.Shared.Pagination;

namespace ProjetoFinal.Domain.Repositories;

public interface IDefaultRepository<TEntity, TFilter, TKey>
    where TEntity : class
    where TFilter : Filter
{
    public Task<TEntity?> FindAsync(TKey id, CancellationToken cancellationToken = default);
    public Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
    public Task<PagedResult<TEntity>> GetAllAsync(TFilter filter, CancellationToken cancellationToken = default);
    public Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    public Task AddRangeAsync(IList<TEntity> entities, CancellationToken cancellationToken = default);
    public Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    public Task UpdateRangeAsync(IList<TEntity> entity, CancellationToken cancellationToken = default);
    public Task<bool> HasAnyAsync(Func<TEntity, bool>? predicate = null, CancellationToken cancellationToken = default);
    public Task<TEntity> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
    public Task SaveChangesAsync(CancellationToken cancellationToken = default);
    public void ChangeTracker();
    public Task DisposeAsync(CancellationToken cancellationToken = default);

    public Task<TEntity?> FirstOrDefaultByPredicateAsync(
        Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    public Task<IList<TEntity>> GetByPredicateAsync(
        Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    public Task<IList<TEntity>> GetByPredicateTrackedAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<TEntity?> FirstOrDefaultByPredicateTrackedAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<int> UpdateByPredicateAsync(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCall,
        CancellationToken cancellationToken = default);

    public Task<int> DeleteByPredicateAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);
}
