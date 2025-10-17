using System;
using ProjetoFinal.Domain.Shared.Filters;

namespace ProjetoFinal.Domain.Filters;

public class ClassGroupFilter : Filter
{
    public Guid? CourseId { get; set; }
    public string? Name { get; set; }
    public bool? RequiresApproval { get; set; }
    public bool? RequiresEnrollmentCode { get; set; }
}
