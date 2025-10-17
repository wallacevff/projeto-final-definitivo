namespace ProjetoFinal.Application.Contracts.Dto;

public class PagedResultDto<TEntity> where TEntity : class
{
    public IList<TEntity> Dados { get; set; } = new List<TEntity>();
    public PageInfoDto PageInfo { get; set; } = new PageInfoDto();
}