using System;
using ProjetoFinal.Domain.Enums;
using ProjetoFinal.Domain.Shared.Filters;

namespace ProjetoFinal.Domain.Filters;

public class ClassEnrollmentFilter : Filter
{
    public Guid? ClassGroupId { get; set; }
    public Guid? StudentId { get; set; }
    public EnrollmentStatus? Status { get; set; }
}
