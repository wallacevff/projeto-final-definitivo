using System;
using ProjetoFinal.Domain.Shared.Filters;

namespace ProjetoFinal.Domain.Filters;

public class ContentVideoAnnotationFilter : Filter
{
    public Guid? ContentAttachmentId { get; set; }
    public Guid? CreatedById { get; set; }
}
