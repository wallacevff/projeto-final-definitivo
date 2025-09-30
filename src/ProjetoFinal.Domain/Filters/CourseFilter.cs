using System;
using ProjetoFinal.Domain.Enums;
using ProjetoFinal.Domain.Shared.Filters;

namespace ProjetoFinal.Domain.Filters;

public class CourseFilter : Filter
{
    public string? Title { get; set; }
    public Guid? InstructorId { get; set; }
    public Guid? CategoryId { get; set; }
    public CourseMode? Mode { get; set; }
    public bool? IsPublished { get; set; }
}
