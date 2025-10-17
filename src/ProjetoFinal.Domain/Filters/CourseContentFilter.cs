using System;
using ProjetoFinal.Domain.Enums;
using ProjetoFinal.Domain.Shared.Filters;

namespace ProjetoFinal.Domain.Filters;

public class CourseContentFilter : Filter
{
    public Guid? CourseId { get; set; }
    public Guid? ClassGroupId { get; set; }
    public Guid? AuthorId { get; set; }
    public ContentItemType? ItemType { get; set; }
    public bool? IsDraft { get; set; }
}
