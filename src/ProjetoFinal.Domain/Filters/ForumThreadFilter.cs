using System;
using ProjetoFinal.Domain.Shared.Filters;

namespace ProjetoFinal.Domain.Filters;

public class ForumThreadFilter : Filter
{
    public Guid? CourseId { get; set; }
    public Guid? ClassGroupId { get; set; }
    public Guid? InstructorId { get; set; }
    public string? Title { get; set; }
    public bool? IsPinned { get; set; }
}
