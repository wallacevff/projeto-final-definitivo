using System;
using ProjetoFinal.Domain.Enums;
using ProjetoFinal.Domain.Shared.Filters;

namespace ProjetoFinal.Domain.Filters;

public class ActivityFilter : Filter
{
    public Guid? CourseId { get; set; }
    public ActivityScope? Scope { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
    public bool? VisibleToStudents { get; set; }
}
