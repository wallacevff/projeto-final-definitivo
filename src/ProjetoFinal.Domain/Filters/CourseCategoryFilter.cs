using ProjetoFinal.Domain.Shared.Filters;

namespace ProjetoFinal.Domain.Filters;

public class CourseCategoryFilter : Filter
{
    public string? Name { get; set; }
    public bool? IsPublished { get; set; }
}
