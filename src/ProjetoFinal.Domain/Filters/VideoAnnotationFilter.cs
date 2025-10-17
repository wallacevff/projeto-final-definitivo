using System;
using ProjetoFinal.Domain.Shared.Filters;

namespace ProjetoFinal.Domain.Filters;

public class VideoAnnotationFilter : Filter
{
    public Guid? SubmissionId { get; set; }
    public Guid? AttachmentId { get; set; }
    public Guid? CreatedById { get; set; }
}
