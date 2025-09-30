using System.Linq.Expressions;
using System.Reflection;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using ProjetoFinal.Domain.Repositories;
using ProjetoFinal.Domain.Shared.Filters;
using ProjetoFinal.Domain.Shared.Pagination;
using ProjetoFinal.Infra.Data.Contexts;

namespace ProjetoFinal.Infra.Data.Repositories;

public partial class DefaultRepository<TEntity, TFilter, TKey>(AppDbContext context)
    : IDefaultRepository<TEntity, TFilter, TKey>
    where TEntity : class
    where TFilter : Filter
{
    protected readonly DbSet<TEntity> DbSet = context.Set<TEntity>();

    public virtual async Task<TEntity?> FindAsync(TKey id, CancellationToken cancellationToken = default)
    {
        if (id is ValueType)
        {
            return await DbSet.FindAsync(id);
        }

        IList<PropertyInfo> properties = id!
            .GetType()
            .GetProperties();
        IList<object> propsObj = new List<object>();
        properties.ForEach(p => { propsObj.Add(p.GetValue(id)!); });
        var arrayOfKeys = propsObj.ToArray();
        return await DbSet.FindAsync(arrayOfKeys);
    }

    public virtual Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var query = ApplyIncludes(DbSet);
        var predicate = GenerateKeyFilter(id);
        return query.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual async Task<PagedResult<TEntity>> GetAllAsync(TFilter filter,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();
        query = ApplyIncludesList(query);
        query = ApplyOrderBy(query);
        query = query.Where(GetFilters(filter));
        IList<TEntity> result = await GenResultList(filter, query, cancellationToken);
        return await GeneratePagedResult(
            result,
            filter
        );
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return (await DbSet.AddAsync(entity, cancellationToken)).Entity;
    }

    public virtual async Task AddRangeAsync(IList<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await DbSet.AddRangeAsync(entities, cancellationToken);
    }

    public virtual Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            DbSet.Update(entity).Entity);
    }

    public Task UpdateRangeAsync(IList<TEntity> entity, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => DbSet.UpdateRange(entity), cancellationToken);
    }

    public virtual Task<bool> HasAnyAsync(Func<TEntity, bool>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            cancellationToken.ThrowIfCancellationRequested();
        }

        if (predicate is null)
            return DbSet.AnyAsync(cancellationToken);
        return Task.FromResult(DbSet.Any(predicate));
    }

    public virtual async Task<TEntity> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            cancellationToken.ThrowIfCancellationRequested();
        }

        return await Task.FromResult(DbSet.Remove(entity).Entity);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
        context.ChangeTracker.Clear();
        GC.Collect();
    }

    public void ChangeTracker()
    {
        context.ChangeTracker.Clear();
    }

    public Task DisposeAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return context.DisposeAsync()
            .AsTask();
    }

    public virtual async Task<IList<TEntity>> GetByPredicateAsync(Expression<Func<TEntity, bool>>
        predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task<IList<TEntity>> GetByPredicateTrackedAsync(Expression<Func<TEntity, bool>>
        predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public Task<TEntity?> FirstOrDefaultByPredicateTrackedAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return DbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual async Task<TEntity?> FirstOrDefaultByPredicateAsync(Expression<Func<TEntity, bool>>
        predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public Task<int> UpdateByPredicateAsync(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<SetPropertyCalls<TEntity>,
            SetPropertyCalls<TEntity>>> setPropertyCall,
        CancellationToken cancellationToken = default)
    {
        return DbSet
            .Where(predicate)
            .ExecuteUpdateAsync(setPropertyCall, cancellationToken);
    }

    public virtual Task<int> DeleteByPredicateAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return DbSet.Where(predicate).ExecuteDeleteAsync(cancellationToken);
    }


    protected virtual IQueryable<TEntity> ApplyIncludes(IQueryable<TEntity> query)
    {
        return query;
    }

    protected virtual IQueryable<TEntity> ApplyIncludesList(IQueryable<TEntity> query)
    {
        return query;
    }

    protected virtual IQueryable<TEntity> ApplyOrderBy(IQueryable<TEntity> query)
    {
        return query;
    }


    protected virtual async Task<PagedResult<TEntity>> GeneratePagedResult(IList<TEntity> entities, TFilter filter)
    {
        var query = ApplyIncludesList(DbSet);
        var totalItens = await query.Where(GetFilters(filter)).CountAsync<TEntity>();
        var totalPages = (int)Math.Ceiling(totalItens / (double)(filter.PageSize > 0 ? filter.PageSize : totalItens));

        return new PagedResult<TEntity>
        {
            PageInfo = new PageInfo
            {
                PageNumber = filter.PageNumber != 0 ? filter.PageNumber : 1,
                PageSize = filter.PageSize != 0 ? filter.PageSize : totalItens,
                TotalPages = filter.PageNumber != 0 &&
                             filter.PageSize != 0
                    ? totalPages
                    : 1,
                TotalItens = totalItens
            },
            Dados = entities
        };
    }

    protected async Task<int> GetTotalItems(TFilter filter)
    {
        return await DbSet.Where(GetFilters(filter)).CountAsync<TEntity>();
    }

    protected virtual async Task<PagedResult<TEntityInternal>> GeneratePagedResult
        <TEntityInternal, TFilterInternal>
        (IList<TEntityInternal> entities, TFilterInternal filter)
        where TEntityInternal : class
        where TFilterInternal : Filter
    {
        var dbSet = context.Set<TEntityInternal>().AsNoTracking();
        var totalItens = await dbSet.Where(GetFiltersInternal<TEntityInternal, TFilterInternal>(filter)).CountAsync();
        var totalPages = (int)Math.Ceiling(totalItens / (double)(filter.PageSize > 0 ? filter.PageSize : totalItens));

        return new PagedResult<TEntityInternal>
        {
            PageInfo = new PageInfo
            {
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalPages = totalPages,
                TotalItens = totalItens
            },
            Dados = entities
        };
    }

    private Expression<Func<TEntity, bool>> GenerateKeyFilter<T>(T ids)
    {
        var predicate = PredicateBuilder.New<TEntity>(true);
        var entityProps = typeof(TEntity).GetProperties();
        var entityIds = typeof(T).GetProperties();
        var idEntity = entityProps.FirstOrDefault(p => string.Equals(p.Name, "Id"));

        if (ids is ValueType && idEntity is not null)
        {
            var param = Expression.Parameter(typeof(TEntity), "p");
            var entityProperty = Expression.Property(param, idEntity.Name);
            var constant = Expression.Constant(ids);
            var body = Expression.Equal(entityProperty, constant);
            predicate.And(Expression.Lambda<Func<TEntity, bool>>(body, param));
            return predicate;
        }

        foreach (var entityId in entityIds)
        {
            var value = entityId.GetValue(ids);
            if (value is not null)
            {
                var param = Expression.Parameter(typeof(TEntity), "p");
                var entityProperty = Expression.Property(param, entityId.Name);
                var constant = Expression.Constant(value);
                var body = Expression.Equal(entityProperty, constant);
                predicate.And(Expression.Lambda<Func<TEntity, bool>>(body, param));
            }
        }

        return predicate;
    }

    protected async Task<IList<TEntity>> GenResultList(TFilter filter, IQueryable<TEntity> query,
        CancellationToken cancellationToken = default)
    {
        List<TEntity> result;
        if (filter.PageNumber == 0 && filter.PageSize == 0)
        {
            result = await query
                .ToListAsync(cancellationToken);
        }
        else
        {
            result = await query
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize).ToListAsync(cancellationToken)
                ;
        }

        return result;
    }

    protected async Task<IList<TInternalEntity>> GenResultList<TInternalEntity, TInternalFilter>
        (TInternalFilter filter, IQueryable<TInternalEntity> query, CancellationToken cancellationToken = default)
        where TInternalEntity : class
        where TInternalFilter : Filter
    {
        List<TInternalEntity> result;
        if (filter.PageNumber == 0 && filter.PageSize == 0)
        {
            result = await query
                .ToListAsync(cancellationToken);
        }
        else
        {
            result = await query
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize).ToListAsync(cancellationToken)
                ;
        }

        return result;
    }
}