namespace ProjetoFinal.Application.Contracts.Dto;

public class PageInfoDto
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalItens { get; set; }
}