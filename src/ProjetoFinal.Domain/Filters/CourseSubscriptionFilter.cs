using System;
using ProjetoFinal.Domain.Shared.Filters;

namespace ProjetoFinal.Domain.Filters;

public class CourseSubscriptionFilter : Filter
{
    public Guid? CourseId { get; set; }
    public Guid? StudentId { get; set; }
}
