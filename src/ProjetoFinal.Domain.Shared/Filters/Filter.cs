namespace ProjetoFinal.Domain.Shared.Filters;

public abstract class Filter
{
    public int PageSize { get; set; } = 10;
    public int PageNumber { get; set; } = 1;
}