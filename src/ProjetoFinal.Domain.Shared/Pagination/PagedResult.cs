namespace ProjetoFinal.Domain.Shared.Pagination;

public class PagedResult<TEntity> where TEntity : class
{
    public required IList<TEntity> Dados { get; set; } = new List<TEntity>();
    public PageInfo PageInfo { get; set; } = new PageInfo();
}