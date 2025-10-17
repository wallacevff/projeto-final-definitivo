namespace ProjetoFinal.Domain.Shared.Pagination;

public class PageInfo
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalItens { get; set; }
}