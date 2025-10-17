using ProjetoFinal.Domain.Enums;
using ProjetoFinal.Domain.Shared.Filters;

namespace ProjetoFinal.Domain.Filters;

public class MediaResourceFilter : Filter
{
    public string? FileName { get; set; }
    public string? Sha256 { get; set; }
    public MediaKind? Kind { get; set; }
}
