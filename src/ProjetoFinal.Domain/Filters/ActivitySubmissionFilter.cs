using System;
using ProjetoFinal.Domain.Enums;
using ProjetoFinal.Domain.Shared.Filters;

namespace ProjetoFinal.Domain.Filters;

public class ActivitySubmissionFilter : Filter
{
    public Guid? ActivityId { get; set; }
    public Guid? StudentId { get; set; }
    public Guid? ClassGroupId { get; set; }
    public SubmissionStatus? Status { get; set; }
}
