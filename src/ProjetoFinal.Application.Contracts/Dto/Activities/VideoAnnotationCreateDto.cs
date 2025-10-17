using System;

namespace ProjetoFinal.Application.Contracts.Dto.Activities;

public class VideoAnnotationCreateDto
{
    public Guid SubmissionId { get; set; }
    public Guid AttachmentId { get; set; }
    public Guid CreatedById { get; set; }
    public double TimeMarkerSeconds { get; set; }
    public string Comment { get; set; } = string.Empty;
}
